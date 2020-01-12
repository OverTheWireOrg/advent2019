from flask import Flask, request, jsonify
import time
from collections import defaultdict
import json
import base64
import os
import random

###########################################################
## IF YOU ARE PLAYTESTING PLEASE STOP READING THE SOURCE ##
##                                              - hpmv   ##
###########################################################

app = Flask(__name__)

game_states = {}

def throttle(pred):
    def decorator(handler):
        def decorated(key, req):
            if pred(req):
                if time.time() < game_states[key].last_req_time + 1:
                    return {'error': 'Throttled at one request per second. ' +
                                'Please note that this challenge does NOT require brute forcing, ' +
                                'and does NOT require sending an excessive number of requests.'}
                game_states[key].last_req_time = time.time()
            return handler(key, req)
        return decorated
    return decorator


def keep_history(endpoint):
    def decorator(handler):
        def decorated(key, req):
            if key is not None:
                history_list = game_states[key].history[endpoint]
                # Prevent flooding of the history list
                if len(history_list) >= 100:
                    del history_list[0]
                game_states[key].history[endpoint].append((int(time.time() * 1000), req))
            return handler(key, req)
        return decorated
    return decorator


class GameState:
    def __init__(self, key, name):
        self.history = defaultdict(list)
        self.last_req_time = 0
        self.state = EncryptState(key, {'name': name, 'money': 0.0, 'speed': 1})


def EncryptState(key, data):
    encoded = json.dumps(data)
    encrypted = ''
    for i, c in enumerate(encoded):
        encrypted += chr(ord(c) ^ ord(key[i % len(key)]))
    return base64.b64encode(encrypted)


def DecryptState(key, encrypted):
    decrypted = ''
    for i, c in enumerate(base64.b64decode(encrypted)):
        decrypted += chr(ord(c) ^ ord(key[i % len(key)]))
    return json.loads(decrypted)


def sha1(data):
    import hashlib
    h = hashlib.new('sha1')
    h.update(data)
    return h.digest()

def CalculateUpgradeCost(current_speed):
    try:
        return 1.1 ** current_speed * 10
    except:
        return 1e300


def GarbageCollectSpamGameStates():
    # As a measure to keep the server running, we cap the number of games to
    # a reasonable amount. When we exceed that amount, we'll clean out the
    # games with the least amount of history.
    if len(game_states) >= 10000:
        keys_to_clean = []
        for key, state in game_states.iteritems():
            keys_to_clean.append((key, len(state.history['control'])))
        keys_to_clean.sort(key = lambda (x,y): y)
        for i in range(5000):
            del game_states[keys_to_clean[i][0]]
        print "Garbage collected game states."


@keep_history('control')
def game_control(key, req):
    action = req['action']
    if action == 'new' and key is None:
        GarbageCollectSpamGameStates()
        key = ''.join(map(chr, [random.choice(range(256)) for _ in range(32)]))
        game_states[key] = GameState(sha1(key), str(req['name']))
        return {'id': base64.b64encode(key)}
    game = game_states[key]
    if action == 'load':
        return game.state
    elif action == 'save':
        game.state = req['data']
    else:
        raise Exception('Invalid action')

@throttle(lambda req: req['action'] != 'state')
@keep_history('client')
def client_handler(key, req):
    action = req['action']
    state = DecryptState(sha1(key), game_control(key, {'action': 'load'}))
    ret = {}
    if action == 'collect':
        state['money'] += state['speed']
    elif action == 'melt':
        state['money'] -= 1
    elif action == 'upgrade':
        upgrade_cost = CalculateUpgradeCost(state['speed'])
        if state['money'] >= upgrade_cost:
            state['money'] -= upgrade_cost
            state['speed'] += 1
    elif action == 'buy_flag':
        if state['money'] >= 1e63:
            state['flag'] = open('flag.txt').read()
            state['money'] -= 1e63
    elif action == 'state':
        ret = {'snowflakes': state['money'], 'collect_speed': state['speed'],
               'elf_name': state['name'],
                'speed_upgrade_cost': CalculateUpgradeCost(state['speed'])}
        if 'flag' in state:
            ret['flag'] = state['flag']
    game_control(
        key, {'action': 'save', 'data': EncryptState(sha1(key), state)})
    return ret


@app.route('/control', methods=['POST'])
def control():
    if not request.json:
        return None
    key = base64.b64decode(request.cookies['id']) if 'id' in request.cookies else None
    return jsonify(game_control(key, request.json))


@app.route('/client', methods=['POST'])
def client():
    if not request.json:
        return None
    key = base64.b64decode(request.cookies['id']) if 'id' in request.cookies else None
    if key not in game_states:
        return jsonify({'not_found': True})
    return jsonify(client_handler(key, request.json))


@app.route('/history/<string:endpoint>', methods=['GET'])
def history(endpoint):
    key = base64.b64decode(request.cookies['id'])
    return jsonify(game_states[key].history[endpoint])


@app.route('/')
def indexhtml():
    return app.send_static_file('index.html')


if __name__ == '__main__':
    app.run(host='0.0.0.0')
