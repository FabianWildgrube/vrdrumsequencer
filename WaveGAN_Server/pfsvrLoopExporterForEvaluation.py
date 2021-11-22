from pydub import *
from donahueDrumSampleGenerator import DonahueDrumGenerator
import json
import sounddevice as sd
import wave
import os
import soundfile as sf
from pathlib import Path

generator = DonahueDrumGenerator()


def linearToDBReduction(unityVolumeValue):
    '''
    Convert values from 1.0 - 0.0 to db reduction.
    I.e.: 1.0 == 0db reduction; 0.5 == -6db; 0.25 == -12db; ...
    :param unityVolumeValue:
    :return:
    '''
    if (unityVolumeValue >= 0.99):
        return 0

    kehrwert = 0.5 / (unityVolumeValue)
    return (kehrwert) * -6.0


def generateSampleSound(inputVector):
    # save to temp wav file and read back in using pydub AudioSegment,
    # because passing the data directly leads to heavily distorted sounds
    generatedSample = generator.generate(inputVector)
    sf.write('temp.wav', generatedSample, 16000)
    # sd.play(generatedSample, samplerate=16000, blocking=True)

    segment = AudioSegment.from_file("temp.wav", format="wav")

    os.remove('temp.wav')

    return segment


def bounceLoop(loopInAbsPath, outputBasePath='', isPerfection=False):
    # read in the loop file
    jsonFile = open(loopInAbsPath)
    loop = json.load(jsonFile)

    if loop['songName'] is None:
        # loops without a song are not to be considered
        print("Loop without a song! " + loop['participantId'] + '-' + loop['conditionName'] + '-' + loop['loopName'])
        return

    # check if loop actually has at least one note
    totalNrOfNotes = 0
    for track in loop['tracks']:
        totalNrOfNotes += len(track['notes'])

    if totalNrOfNotes == 0:
        # loops without a single note are not to be considered
        print("Loop without notes! " + loop['participantId'] + '-' + loop['conditionName'] + '-' + loop['loopName'])
        return

    # including the loopName ensures any previous versions of the loop are overwritten on export,
    # leaving only the last version
    baseFileName = outputBasePath + loop['participantId'] + '-' + loop['conditionName'] + '-' + loop['loopName'] + '-p' if isPerfection else ''

    # prepare base audio to overlay notes on
    secondsPerBeat = 60.0 / loop['bpm'] * (4.0 / loop['timeSignatureLo'])
    secondsPerBar = secondsPerBeat * loop['timeSignatureHi']
    durationInSeconds = loop['durationInBars'] * secondsPerBar
    loopAssembly = AudioSegment.silent(duration=durationInSeconds * 1000, frame_rate=16000)

    # place all notes of every track in the assembly
    for track in loop['tracks']:
        if not track['isMuted']:
            sample = generateSampleSound(track['sampleDefinition']['_vectorValues'])
            sample = sample + linearToDBReduction(track['trackVolume'])
            for note in track['notes']:
                loopAssembly = loopAssembly.overlay(sample, position=note['triggerTime'] * 1000)

    # export loop to wav file
    loopAssembly.export(baseFileName + '.wav', format='wav')

    loop['isPerfectionLoop'] = isPerfection
    # export loop json to file with same name for convenience in evaluation tools
    with open(baseFileName + '.json', 'w') as jsonOutputFile:
        json.dump(loop, jsonOutputFile, indent=4)


# bounce all user loops
basePath = '..\\surveyRawData'

outputDir = '..\\surveyRawData\\SongExports\\'

Path(outputDir).mkdir(parents=True, exist_ok=True)

for participantFolder in os.listdir(basePath):
    if '_all' in participantFolder or 'SongExports' in participantFolder or not os.path.isdir(
            basePath + '\\' + participantFolder):
        continue  # ignore crashed participants


    for conditionsFolderName in os.listdir(basePath + '\\' + participantFolder):
        conditionsFolderPath = basePath + '\\' + participantFolder + '\\' + conditionsFolderName
        if conditionsFolderName != 'Loops' and os.path.isdir(conditionsFolderPath):
            # perfection loops
            for loopFileName in os.listdir(conditionsFolderPath + '\\Perfection_Loops'):
                bounceLoop(conditionsFolderPath + '\\Perfection_Loops\\' + loopFileName, outputDir, isPerfection=True)
                print("Bounced Perfection Loop " + loopFileName)

            # exploration loops
            for loopFileName in os.listdir(conditionsFolderPath + '\\Exploration_Loops'):
                bounceLoop(conditionsFolderPath + '\\Exploration_Loops\\' + loopFileName, outputDir)
                print("Bounced Exploration Loop " + loopFileName)
