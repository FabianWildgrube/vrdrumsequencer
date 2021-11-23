# WaveGAN server for PFS-VR
Websocket server wrapper for the Donahue Drums GAN.

> Slight modification of the server scripts taken from: https://hcai.eu/git/project/pufferfishsynth, sub-folder `GAN`.
Python wrapper code originally written by Silvan Mertes and Ruben Schlagowski for the publication: Schlagowski, Ruben, Silvan Mertes, and Elisabeth André. "Taming the chaos: exploring graphical input vector manipulation user interfaces for GANs in a musical context." Audio Mostly 2021. 2021. 216-223.

The WaveGAN that generates drum samples was created by Donahue et al.:
Donahue, Chris, Julian McAuley, and Miller Puckette. "Adversarial audio synthesis." arXiv preprint arXiv:1802.04208 (2018).

Their code is available at https://github.com/chrisdonahue/wavegan.

## Running the server
Run the `pfsvrServer.py` script. This will start a Websocket Server listening on `localhost:9876`
(this can be changed through the `_IP` and `_PORT` variables in the script)

The server will generate a sound upon receiving a message with a JSON-object of the schema:
```JSON
{
    "id": "...",
    "Items": ["...100 float values..."]
}
```

The generated sound is returned to the requesting client as an uncompressed WAV (an array of float values with a frequency of 16000 samples/s).