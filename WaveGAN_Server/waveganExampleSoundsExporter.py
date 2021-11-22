from donahueDrumSampleGenerator import DonahueDrumGenerator
import json
import soundfile as sf
import numpy as np

generator = DonahueDrumGenerator()


def generateSampleSound(inputVector, name):
    # save to temp wav file and read back in using pydub AudioSegment,
    # because passing the data directly leads to heavily distorted sounds
    generatedSample = generator.generate(inputVector)
    sf.write(name + '.wav', generatedSample, 16000)


# generate 4 random samples and export them
for i in range(1, 5):
    values = np.random.uniform(low=0.0, high=1.0, size=(100,))
    generateSampleSound(values, "Random" + str(i))

# generate all the sounds from the sampleLibrary
jsonFile = open('defaultSampleLibrary.json')
sampleLibrary = json.load(jsonFile)

sampleDefinitions = sampleLibrary['_sampleDefinitions']
for definition in sampleDefinitions.values():
    generateSampleSound(definition['_vectorValues'], definition['name'])
