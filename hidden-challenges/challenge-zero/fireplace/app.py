#!/usr/bin/env python3

from glob import glob
import itertools
import os
import os.path
import re

from gevent import sleep
from gevent.pywsgi import WSGIServer
from bottle import Bottle, request, static_file


HOST = os.getenv('HOST', 'localhost')
PORT = os.getenv('PORT', 10000)
DEBUG = os.getenv('DEBUG')
app = Bottle()


QEMU_HINT = 'Hint: qemu-system-x86_64 boot.bin -cpu max -s'
QEMU_HINT2 = "Hint: Pause qemu by add -S to the args and type 'c' in the monitor"
QEMU_HINT3 = 'Hint: $ target remote localhost:1234'
QEMU_HINT4 = 'Hint: $ break *0x7c00'
FIRE_HINT = 'Hint: Try reading between the lines.'
FIRE_HINT2 = "Hint: If only the flames wouldn't move that much..."
TEXT_OPERA = 'Put your hands up, this is the Chrome Shop mafia!'
TEXT_IE_EDGE = "This is quite the browser safari, don't you agree?"
TEXT_FIREFOX = 'Did you know: Plain text goes best with a text browser.'
TEXT_CHROME = 'Fox! Fox! Burning bright! In the forests of the night!'
TEXT_SAFARI = 'Opera: Music for the masses'
TEXT_BROWSER = 'D0NT PU5H M3 C0Z 1M C1053 T0 T3H 3DG3'
TEXT_WGET = 'Is that a curling iron in your pocket or are you just happy to see me?'
TEXT_UNKNOWN = "I don't know who you are. Try using a more popular browser."
APP_DIR = os.path.dirname(os.path.realpath(__file__))
FIRE_FRAMES_DIR = os.path.join(APP_DIR, 'frames')
FIRE_FRAME_PATHS = glob(os.path.join(FIRE_FRAMES_DIR, '*.ansi'))
FIRE_FRAMES = [open(f).read() for f in sorted(FIRE_FRAME_PATHS)]
GIF = '<img style="width:400px;" src="/flames.gif">'


def is_browser(regex):
    ua = request.headers.get('User-Agent', '')
    return re.search(regex, ua.lower())


def browser_response(browser, text, hint, animation=True):
    return '''
    <html>
      <head>
        <title>Fireplace</title>
        <!-- browser detected: {} -->
      </head>
      <body>
        {}<pre>{}\n\n{}</pre>
      </body>
    </html>
    '''.format(browser, GIF if animation else '', text, hint)


def fire_animation():
    for frame in itertools.cycle(FIRE_FRAMES):
        yield(frame)
        sleep(0.2)
        yield '\033[2J\033[H\n'


@app.route('/')
def index():
    if is_browser('opera|opr|opios'):
        return browser_response('opera', TEXT_OPERA, QEMU_HINT)
    elif is_browser('msie|trident|\\sedg/|edge|edga|edgios'):
        return browser_response('ms', TEXT_IE_EDGE, QEMU_HINT2)
    elif is_browser('firefox|iceweasel|fxios'):
        return browser_response('firefox', TEXT_FIREFOX, QEMU_HINT3)
    elif is_browser('chromium|chrome|crios|crmo'):
        return browser_response('chrome', TEXT_CHROME, QEMU_HINT4)
    elif is_browser('safari|applewebkit'):
        return browser_response('safari', TEXT_SAFARI, FIRE_HINT)
    elif is_browser('w3m|elinks|links|lynx'):
        return browser_response('text', TEXT_BROWSER, FIRE_HINT2, animation=False)
    elif is_browser('wget'):
        return TEXT_WGET + '\n'
    elif is_browser('curl'):
        return fire_animation()
    else:
        return browser_response('unknown', TEXT_UNKNOWN, '')


@app.route('/flames.gif')
def flames():
    return static_file('flames.gif', APP_DIR)


if __name__ == '__main__':
    WSGIServer((HOST, PORT), app).serve_forever()
