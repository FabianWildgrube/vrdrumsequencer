import tensorflow as tf
import numpy as np
import librosa


class DonahueDrumGenerator:

    def __init__(self):
        tf.reset_default_graph()

        self.saver = tf.train.import_meta_graph('./models/donahue_drums/infer.meta')
        self.graph = tf.get_default_graph()
        self.sess = tf.InteractiveSession()
        self.saver.restore(self.sess, './models/donahue_drums/model.ckpt')

    def generate(self, inputVector):
        """
        Generates a drum sample.

        :param inputVector: a list of 100 floats representing the input vector for the GAN
        :type inputVector: list
        :return:  a generated 16000 wave sound
        """

        z = self.graph.get_tensor_by_name('z:0')
        G_z = self.graph.get_tensor_by_name('G_z:0')

        # wrap input vector into an ndarray with shape (1, 100)
        # because the generator could generate multiple samples at once if more than one vector are passed in
        _G_z = self.sess.run(G_z, {z: np.array([inputVector])})

        librosa.output.write_wav("mostrecentPFSVRSample.wav", _G_z[0], 16000)

        return _G_z[0, :, 0]  # return the sample. Needs to be extracted from the first index, see comment above
