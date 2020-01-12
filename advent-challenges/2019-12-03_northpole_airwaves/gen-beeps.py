#!/usr/bin/env python

import sys, binascii


def hexrc(data):
    # rc car encoding:
    # pulse of length 4, the a number of bits of length 1

    def conv_hd(h):
        n = "0123456789abcdef".index(h)
        out = "11110" + ("10" * n) + ("0" * 8)
        return out


    print "Data = %s" % data
    hdata = binascii.hexlify(data)
    print "HexData = %s" % hdata
    pulses = "".join([conv_hd(x) for x in hdata])
    return pulses

def hexmorse(data):
    letters_to_morse = {
            "0": "-----",
            "1": ".----",
            "2": "..---",
            "3": "...--",
            "4": "....-",
            "5": ".....",
            "6": "-....",
            "7": "--...",
            "8": "---..",
            "9": "----.",
            "a": ".-",
            "b": "-...",
            "c": "-.-.",
            "d": "-..",
            "e": ".",
            "f": "..-.",
    }

    # dots = 1
    # dash = 111
    # space between dots/dashes: 0
    # space between letters: 000
    # space between words: 0000000

    morse_to_pulses = {
            ".": "10",
            "-": "1110",
    }

    out = []
    print "Data = %s" % data
    hdata = binascii.hexlify(data)
    print "HexData = %s" % hdata
    pulses = [letters_to_morse[l] for l in hdata]
    print "Pulses = %s" % " ".join(pulses)
    for lm in pulses:
        bits = "".join([morse_to_pulses[x] for x in lm])
        out += [bits]
    return "000".join(out) + "0000000"

def bits_to_bytes(bits):
    conv = {
            "0": "\x00\x00",
            "1": "\x01\x01",
    }

    return "".join([conv[x] for x in bits])

filename = sys.argv[1]
ttt = sys.argv[2]
text = sys.argv[3]

if ttt == "morse":
    pulses = hexmorse(text)
else:
    pulses = hexrc(text)

print "morsedata = %s" % pulses

with open(filename, "wb") as fp:
    fp.write(bits_to_bytes(pulses))

