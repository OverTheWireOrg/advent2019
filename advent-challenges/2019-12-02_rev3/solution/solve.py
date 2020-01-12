# THE SOLUTION IS NOT YET COMPLETE. STILL WORKING ON IT.

from pwn import *

keystream = []

def dumpkey():
    print hexdump(map(chr, [x or 0 for x in keystream]))

def learn_xor(offset, data):
    global keystream
    if len(keystream) < offset + len(data):
        keystream = keystream + [None] * (offset + len(data) - len(keystream))
    for i in range(offset, offset+len(data)):
        if keystream[i] is not None:
            assert keystream[i] == ord(data[i-offset])
    keystream[offset:offset + len(data)] = map(ord, data)

def new_client():
    #return remote('localhost', 50002)
    return remote('34.198.170.179', '35002')

def new_server():
    #return remote('localhost', 50001)
    return remote('34.198.170.179', '35001')


def fake_remote_initial():
    p = new_client()
    print 'Connect using:'
    print p.recvline()
    p.recvline()
    p.send('\x00' * 16)
    print 'Enter username %s and password %s and press Login' % ("A"*16, "B"*256)
    raw_input("Done?")
    data1 = p.recvrepeat(0.1)
    p.close()

    print '====================='

    p = new_client()
    print 'Reconnect using:'
    print p.recvline()
    p.recvline()
    p.send('\x00' * 16)
    print 'Enter username %s and password %s and press Login' % ("A"*16, "C"*256)
    raw_input("Done?")
    data2 = p.recvrepeat(0.1)
    p.close()

    for i in range(len(data1)):
        if data1[i] != data2[i]:
            learn_xor(i, xor(data1[i:i+256], 'B'))

    open('stage1','w').write(repr(
        (data1, data2, keystream)))
    
def real_server_initial():
    global keystream
    (data1, data2, keystream) = eval(open('stage1').read())

    # Register this user with the server if it didn't already exist
    p = new_server()
    server_key = p.recvn(16)
    p.send(xor(data1, server_key))
    p.recvrepeat(0.5)
    p.close()

    # now produce an invalid login
    p = new_server()
    server_key = p.recvn(16)
    p.send(xor(data2, server_key))
    serverdata = p.recvrepeat(0.5)
    p.close()

    serverdata = xor(serverdata, server_key)[:len(serverdata)]

    print hexdump(serverdata)

    dumpkey()

    p = new_client()
    print 'Connect using:'
    print p.recvline()
    p.recvline()
    p.send('\x00' * 16)
    print 'Enter username %s and password %s and press Login' % ("A"*16, "XXXX")
    raw_input("Done?")
    data3 = p.recvrepeat(0.1)
    p.send(serverdata)
    print 'Login again using username %s password %s' % ("A"*16, "XXXX")
    raw_input("Done?")
    data4 = p.recvrepeat(0.1)
    offset = len(data3)
    print offset
    data4key = map(chr, keystream[offset:])
    loginstream = xor(data4, data4key)[:len(data4)]
    print hexdump(loginstream)

    assert len(loginstream) == len(data3)
    learn_xor(0, xor(loginstream, data3))

    long_login = xor(map(chr, keystream), data1)
    login_wrong = xor(serverdata, map(chr, keystream)[:4])
    print hexdump(long_login)
    p.close()

    print '======================'
    p = new_client()
    print 'Reconnect using:'
    print p.recvline()
    p.recvline()
    p.send('\x00' * 16)

    dumpkey()

    print '***Repeat this 20 times:*** Enter username %s and password %s and press Login' % ("A"*16, "B"*256)
    server_counter = 0
    client_counter = 0
    for i in range(40):
        print '%s times remaining' % (40 - i)
        client_data = p.recvn(len(long_login))
        p.send(xor(login_wrong, map(chr, keystream[server_counter:server_counter+len(login_wrong)])))
        learn_xor(client_counter, xor(client_data, long_login))
        client_counter += len(client_data)
        server_counter += len(login_wrong)
    
    open('keystream', 'w').write(''.join(map(chr, keystream)))

class Channel:
    def __init__(self, key, name, recv):
        self.key = key
        self.name = name
        self.counter = 0
        self.recv = recv
        
    def enc(self, data):
        key = keystream[self.counter:self.counter + len(data)]
        encrypted = map(ord, xor(data, map(chr, key)))
        for i in range(self.counter, self.counter + len(data)):
            encrypted[i-self.counter] ^= ord(self.key[i % 16])
        self.counter += len(data)
        return ''.join(map(chr, encrypted))

    def recvmsg(self, p):
        assert self.recv, "Channel is set up for send only"
        datalen_enc = p.recvn(2)
        datalen = self.enc(datalen_enc)
        data_enc = p.recvn(u16(datalen))
        data = self.enc(data_enc)
        print self.name, 'recv:', data.encode('hex')
        return data

    def sendmsg(self, p, msg):
        assert not self.recv, "Channel is set up for recv only"
        print self.name, 'send:', msg.encode('hex')
        p.send(self.enc(p16(len(msg)) + msg))

def forward_sockets():
    global keystream
    keystream = map(ord, open('keystream').read())
    dumpkey()

    serverp = new_server()
    key = serverp.recvn(16)
    client_send = Channel(key, 'client', False)
    client_recv = Channel(key, 'client', True)
    server_send = Channel(key, 'server', False)
    server_recv = Channel(key, 'server', True)

    clientp = new_client()
    print 'Connect using:'
    print clientp.recvline()
    clientp.recvline()
    clientp.send(key)

    while True:
        data = client_recv.recvmsg(clientp)
        server_send.sendmsg(serverp, data)

        data = server_recv.recvmsg(serverp)
        client_send.sendmsg(clientp, data)
    
def probe_more_keystream():
    global keystream
    keystream = map(ord, open('keystream').read())
    dumpkey()
    # login stream
    specific_login = open('specific_login').readlines()
    login_stream = specific_login[0].strip().decode('hex')
    repeat_req = specific_login[1].strip().decode('hex')
    repeat_resp = specific_login[2].strip().decode('hex')
    repeat_resp_with_len = p16(len(repeat_resp)) + repeat_resp

    p = new_server()
    key = p.recvn(16)
    chan_recv = Channel(key, 'server', True)
    chan_send = Channel(key, 'server', False)
    chan_send.sendmsg(p, login_stream)
    game_msg = chan_recv.recvmsg(p)
    assert len(game_msg) > 100, "login failed"
    while chan_recv.counter < 900000:
        for i in range(50):
            chan_send.sendmsg(p, repeat_req)
        for i in range(50):
            resp_enc = p.recvn(len(repeat_resp_with_len))
            recovered_key = map(ord, xor(resp_enc, repeat_resp_with_len))
            for i in range(len(recovered_key)):
                recovered_key[i] ^= ord(key[(i + chan_recv.counter) % 16])
            learn_xor(chan_recv.counter, ''.join(map(chr, recovered_key)))
            chan_recv.counter += len(resp_enc)
    open('keystream_long', 'w').write(''.join(map(chr, keystream)))


def forward_sockets_with_mutation():
    global keystream
    keystream = map(ord, open('keystream_long').read())

    serverp = new_server()
    key = serverp.recvn(16)
    client_send = Channel(key, 'client', False)
    client_recv = Channel(key, 'client', True)
    server_send = Channel(key, 'server', False)
    server_recv = Channel(key, 'server', True)

    clientp = new_client()
    print 'Connect using:'
    print clientp.recvline()
    clientp.recvline()
    clientp.send(key)

    while True:
        data = client_recv.recvmsg(clientp)
        if data.encode('hex') == '22021001':
            data = '2200'.decode('hex')
        server_send.sendmsg(serverp, data)

        data = server_recv.recvmsg(serverp)
        # replace this with whatever you see for the real data.
        if data.encode('hex') == '0abe1908011214080610c10218d804200628a40130c00238bec0261a5b121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea3012170808220f080210011880d39dfbffffffffff012202080218061a5b0801121b08011014220f08021001189cffffffffffffffff01220408021850121d080618a08d06220f0802100118f6ffffffffffffffff0122040802180a1219100a220f0802100118f6ffffffffffffffff0122040802180818201ae9170802121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea301219100a220f0802100118f6ffffffffffffffff01220408021808121b08011014220f08021001189cffffffffffffffff01220408021850121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea301219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff01220408021808121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea3018e807':
            data = '0abe1908011214080610c10218d804200628a40130c00238bec0261a5b121e080710c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea3012170808220f080210011880d39dfbffffffffff012202080218061a5b0801121b08011014220f08021001189cffffffffffffffff01220408021850121d080618a08d06220f0802100118f6ffffffffffffffff0122040802180a1219100a220f0802100118f6ffffffffffffffff0122040802180818201ae9170802121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea301219100a220f0802100118f6ffffffffffffffff01220408021808121b08011014220f08021001189cffffffffffffffff01220408021850121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea301219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff012204080218081219100a220f0802100118f6ffffffffffffffff01220408021808121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121b08011014220f08021001189cffffffffffffffff01220408021850121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08021028220f080210011898f8ffffffffffffff012205080218a006121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121c08031050220f0802100118f0b1ffffffffffffff012205080218c03e121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080410a001220f0802100118e0f2f9ffffffffffff01220608021880f104121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea30121e080510c002220f0802100118c0fbc2ffffffffffff01220608021880ea3018e807'.decode('hex')
        client_send.sendmsg(clientp, data)

# TODO(hpmv): Do more...

# step 1
#fake_remote_initial()

# step 2
#real_server_initial()

# step 3, need to record the login stream and a repeatable request/response stream.
#forward_sockets()

# step 4
#probe_more_keystream()

# step 5
#forward_sockets_with_mutation()