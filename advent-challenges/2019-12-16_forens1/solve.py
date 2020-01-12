import mido

m = mido.MidiFile('Stegno.mid')

curtime = 0

offbeats = []
LOOKUP = {
    0: 0,
    2: 1,
    4: 2,
    5: 3,
    7: 4,
    9: 5,
    11: 6
}

for msg in m:
    if msg.type != 'note_on': continue
    channel = msg.channel
    time = msg.time / 1.09091 * 8
    if msg.velocity > 0:
        if int(curtime + time+0.1) % 2 == 1:
            offbeats.append(LOOKUP[(msg.note - 31) % 12])
    curtime += time

flag = ''
for i in range(len(offbeats)/3):
    char = chr(int(''.join(map(str,offbeats[i*3:][:3])), 7))
    flag += char
print flag
