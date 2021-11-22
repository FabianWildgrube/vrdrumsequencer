from pydub import *
from donahueDrumSampleGenerator import DonahueDrumGenerator
import json
import sounddevice as sd
import wave
import os
import soundfile as sf
from pathlib import Path

generator = DonahueDrumGenerator()

songDir = "C:\\UnityProjects\\MAThesisRepo\\PFS-VR_CreativityPrototype\\Assets\\SongRawClips_quieter"

exportPerfection = True
exportExploration = True


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


def bounceLoop(loopInAbsPath, outputBasePath='', includeLoopNameInFileName=True):
    # read in the loop file
    jsonFile = open(loopInAbsPath)
    loop = json.load(jsonFile)

    if loop['songName'] is None:
        # loops without a song are not to be considered
        return

    baseFileName = outputBasePath + loop['participantId'] + '-' + loop['conditionName'] + (
        ('-' + loop['loopName']) if includeLoopNameInFileName else '')

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

    # export loop without song
    loopAssembly.export(baseFileName + '-bare.mp3', format='mp3')

    # load song
    songLibraryJsonfile = open('SongLibrary.json')
    songLibrary = json.load(songLibraryJsonfile)

    songClipNameMapping = next(filter(lambda stc: stc['songName'] == loop['songName'], songLibrary['mappings']))
    songSegment = AudioSegment.from_file(songDir + '\\' + songClipNameMapping['clipFileName'] + '.wav', format="wav")

    # song volume
    volume = linearToDBReduction(loop['songVolume'])
    songSegment = songSegment + volume  # make song slightly less loud so beat can be heard better

    # overlay loop onto song
    songLoopAssembly = songSegment.overlay(loopAssembly, loop=True)

    # export complete song+loop
    songLoopAssembly.export(baseFileName + '.mp3', format='mp3')


# bounce all user loops
basePath = 'C:\\Users\\admin\OneDrive\\Studium\\MASTER\\04_MasterThesis\\11_Hauptstudie\\04_Rohdaten'
excludeFolder = '001-BackupsOTree'

outputDir = 'C:\\Users\\admin\\OneDrive\\Studium\\MASTER\\04_MasterThesis\\11_Hauptstudie\\05_SongExports\\'

outputDirExploration = outputDir + 'exploration\\'
outputDirPerfection = outputDir + 'perfection\\'
Path(outputDirExploration).mkdir(parents=True, exist_ok=True)
Path(outputDirPerfection).mkdir(parents=True, exist_ok=True)

for participantFolder in os.listdir(basePath):
    if '_all' in participantFolder or excludeFolder == participantFolder:
        continue  # ignore crashed participants for now

    for conditionsFolderName in os.listdir(basePath + '\\' + participantFolder):
        conditionsFolderPath = basePath + '\\' + participantFolder + '\\' + conditionsFolderName
        if conditionsFolderName != 'Loops' and os.path.isdir(conditionsFolderPath):
            # perfection loops
            if exportPerfection:
                for loopFileName in os.listdir(conditionsFolderPath + '\\Perfection_Loops'):
                    bounceLoop(conditionsFolderPath + '\\Perfection_Loops\\' + loopFileName, outputDirPerfection,
                               includeLoopNameInFileName=False)
                    print("Bounced Loop " + loopFileName)

            # exploration loops
            if exportExploration:
                for loopFileName in os.listdir(conditionsFolderPath + '\\Exploration_Loops'):
                    bounceLoop(conditionsFolderPath + '\\Exploration_Loops\\' + loopFileName, outputDirExploration)
                    print("Bounced Loop " + loopFileName)
