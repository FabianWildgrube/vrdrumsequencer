from SimpleWebSocketServer import SimpleWebSocketServer, WebSocket
from donahueDrumSampleGenerator import DonahueDrumGenerator
import struct
import io
import json
import sys
import sounddevice as sd

_DEFAULT_VECTOR = [0.5873295664787293, 0.6972805261611939, 0.15056180953979493, 0.5395260453224182,
                   0.9179067611694336,
                   0.09049248695373535, 0.44609421491622927, 0.8221858739852905, 0.08961606025695801,
                   0.010547041893005371,
                   0.5497227311134338, 0.2140178680419922, 0.7587383985519409, 0.7761831283569336,
                   0.07989895343780518,
                   0.54665607213974, 0.5254426598548889, 0.30940407514572146, 0.4564434885978699,
                   0.8913878202438355,
                   0.6853374242782593, 0.2635403275489807, 0.6475785970687866, 0.5285665392875671,
                   0.4965159296989441,
                   0.16272902488708497, 0.4467547535896301, 0.9146943092346191, 0.8198674917221069,
                   0.24936413764953614,
                   0.1530689001083374, 0.27205199003219607, 0.027986645698547365, 0.9918680191040039,
                   0.5375275015830994,
                   0.11260020732879639, 0.1389375925064087, 0.30683833360671999, 0.34935182332992556,
                   0.7850461006164551,
                   0.893085241317749, 0.908753514289856, 0.09080326557159424, 0.3624153733253479,
                   0.07586753368377686,
                   0.240775465965271, 0.09939682483673096, 0.6321521997451782, 0.8267718553543091,
                   0.39671140909194949,
                   0.6207858920097351, 0.9525721073150635, 0.8797630071640015, 0.16294240951538087,
                   0.605540931224823,
                   0.7691259384155273, 0.9842056035995483, 0.9031579494476318, 0.24763906002044679,
                   0.22670292854309083,
                   0.42008668184280398, 0.296999990940094, 0.5871432423591614, 0.9200938940048218,
                   0.3447790741920471,
                   0.5504177212715149, 0.6503406763076782, 0.2003638744354248, 0.27298682928085329,
                   0.7520860433578491,
                   0.3533005118370056, 0.3947222828865051, 0.9028208255767822, 0.8310497999191284,
                   0.7190036773681641,
                   0.6799554824829102, 0.37569576501846316, 0.6911588907241821, 0.6605329513549805,
                   0.036450982093811038,
                   0.14664077758789063, 0.7068012952804565, 0.42486876249313357, 0.9640188217163086,
                   0.9495252370834351,
                   0.4046798348426819, 0.9711130857467651, 0.46378105878829958, 0.3060910105705261,
                   0.5333097577095032,
                   0.8943504095077515, 0.27032512426376345, 0.48487573862075808, 0.20658743381500245,
                   0.6502430438995361,
                   0.6231829524040222, 0.3494648337364197, 0.46808403730392458, 0.17609870433807374,
                   0.6608748435974121]

_IP_ADDRESS = '127.0.0.1'
_PORT = 9876

generator = DonahueDrumGenerator()
generator.generate(_DEFAULT_VECTOR)


class SendBackWavFile(WebSocket):

    def handleMessage(self):
        """
        Expects incoming client messages that contain only a json encoded string of the format:
        {
            "id": <4 byte int>,
            "Items":[...100 float values...]
        }
        Generates a drum sample using the Array in "Items" as input for the generator GAN
        Sends the id (4 byte int) followed by the generated sound (arbitrary 4-byte float values representing a 16000Hz
                      uncompressed sound file) as raw bytes back to the client.
        """

        print("Received request: ", self.data)
        try:
            newSampleRequest = json.load(io.StringIO(self.data))
            generationInputVector = newSampleRequest["Items"]
            requestId = newSampleRequest["id"]
            generatedSample = generator.generate(generationInputVector)
            response = bytearray(struct.pack("i", requestId))
            response.extend(generatedSample.tobytes())
            self.sendMessage(response)
            #sd.play(generatedSample, samplerate=16000, blocking=False)
        except AttributeError as err:
            print("Attribute Error {0}".format(err))
        except:
            print("Unexpected error:", sys.exc_info()[0])

    def handleConnected(self):
        print(self.address, 'connected')

    def handleClose(self):
        print(self.address, 'closed')


server = SimpleWebSocketServer(_IP_ADDRESS, _PORT, SendBackWavFile)
print("Server running")
server.serveforever()
