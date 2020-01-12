# THIS IS OUTDATED.

from pwn import *


def inst_push(i):
    assert i >= 0 and i < 0x80, i
    return chr(i)


def inst_topneg():
    return chr(0x80)


def inst_not():
    return chr(0x81)


def inst_and():
    return chr(0x82)


def inst_or():
    return chr(0x83)


def inst_xor():
    return chr(0x84)


def inst_reverse():
    return chr(0x90)


def inst_dup():
    return chr(0x91)


def inst_def():
    return chr(0xa0)


def inst_emit():
    return chr(0xb0)


def inst_call(i):
    assert i >= 0 and i < 0x20, i
    return chr(0xc0 + i)


def inst_condcall(i):
    assert i >= 0 and i < 0x20, i
    return chr(0xe0 + i)


DEBUG_INST_NAMES = {}


class VM:
    def __init__(self, debug_on=False):
        self._insts = []
        self._stack = []
        self._defs = [[] for _ in range(20)]
        self._output = ''
        self._debug_on = debug_on
        self._debug_depth = 0

    def _pop(self):
        assert len(self._stack), "stack empty!"
        return self._stack.pop()

    def _push(self, value):
        self._stack.append(value)

    def _debug(self, text, params):
        if not self._debug_on:
            return
        if not text.startswith('call'):
            return
        print '-- Stack:', ' '.join(map(lambda x: '%x' % x, reversed(self._stack)))
        # print hexdump(''.join(chr(x) for x in self._insts if x is not None))
        print ' ' * self._debug_depth + text,
        for i in range(params):
            print '%x' % self._stack[-i-1],
        print

    def ExecuteOneInst(self, inst):
        if inst is None:
            self._debug_depth -= 2
            return
        assert inst >= 0 and inst < 0x100, inst
        if inst < 0x80:
            self._debug('push %x' % inst, 0)
            self._push(inst)
            return
        if inst == 0x80:
            self._debug('negtop', 0)
            self._push(self._pop() ^ 0x80)
            return
        if inst == 0x81:
            self._debug('not', 1)
            val = self._pop()
            if val == 0:
                self._push(0xff)
            else:
                self._push(0)
            return
        if inst == 0x82:
            self._debug('and', 2)
            self._push(self._pop() & self._pop())
            return
        if inst == 0x83:
            self._debug('or', 2)
            self._push(self._pop() | self._pop())
            return
        if inst == 0x84:
            self._debug('xor', 2)
            self._push(self._pop() ^ self._pop())
            return
        if inst == 0x90:
            self._debug('reverse', 2)
            size = self._pop() * 0x100 + self._pop()
            vals = []
            for i in range(size):
                vals.append(self._pop())
            for val in vals:
                self._push(val)
            return
        if inst == 0x91:
            self._debug('dup', 1)
            val = self._pop()
            self._push(val)
            self._push(val)
            return
        if inst == 0xa0:
            self._debug('def', 3)
            index = self._pop()
            assert index >= 0 and index < 0x20, index
            size = self._pop() * 0x100 + self._pop()
            code = []
            for i in range(size):
                code.append(self._pop())
            self._defs[index] = code
            return
        if inst == 0xb0:
            self._debug('emit', 1)
            self._output += chr(self._pop())
            return
        if inst >= 0xc0 and inst < 0xe0:
            index = inst - 0xc0
            self._debug('call %s' % DEBUG_INST_NAMES[index], 0)
            self._call(index)
            return
        if inst >= 0xe0:
            index = inst - 0xe0
            self._debug('call? %s' % DEBUG_INST_NAMES[index], 1)
            cond = self._pop()
            if cond != 0:
                self._call(index)
            return
        assert False, 'Invalid instruction %s' % inst

    def _call(self, index):
        is_tail_call = self._insts[:1] == [None]
        if is_tail_call:
            if self._debug_on:
                print ' ' * self._debug_depth + '-- (tail call) --'
            self._insts = self._defs[index] + self._insts
        else:
            self._insts = self._defs[index] + [None] + self._insts
            self._debug_depth += 2

    def Run(self, insts):
        self._insts = map(ord, insts)
        while len(self._insts):
            inst = self._insts[0]
            self._insts = self._insts[1:]
            self.ExecuteOneInst(inst)
        self._debug('', 0)
        return self._output


QUINE_INST_PRINT = 2
QUINE_INST_PRINT_ESCAPE = 3
QUINE_INST_PRINT_NOT_ESCAPE = 4
QUINE_INST_ACTUAL_PRINT = 5
QUINE_INST_PAYLOAD = 6
QUINE_INST_PRINT_ESCAPE_CHAR = 7
QUINE_INST_ACTUAL_PRINT_ESCAPE_CHAR = 8
QUINE_INST_PRINT_ESCAPE_SEQ = 9
QUINE_MARKER_1 = 0x4f
QUINE_MARKER_2 = 0x3f

DEBUG_INST_NAMES[QUINE_INST_PAYLOAD] = 'payload'


def quine_marker_1_ind():
    return (
        inst_push(0x2c) +
        inst_push(0x2c ^ QUINE_MARKER_1) +
        inst_xor()
    )


def quine_marker_2_ind():
    return (
        inst_push(0x2c) +
        inst_push(0x2c ^ QUINE_MARKER_2) +
        inst_xor()
    )


def quine_print():
    return (
        inst_dup() +
        quine_marker_2_ind() +
        inst_xor() +
        inst_dup() +
        inst_not() +
        inst_condcall(QUINE_INST_PRINT_ESCAPE) +
        inst_condcall(QUINE_INST_PRINT_NOT_ESCAPE)
    )


def quine_print_not_escape():
    return (
        inst_dup() +
        quine_marker_1_ind() +
        inst_xor() +
        inst_condcall(QUINE_INST_ACTUAL_PRINT)
    )


def quine_actual_print():
    return (
        inst_emit() +
        inst_call(QUINE_INST_PRINT)
    )


def quine_print_escape():
    return (
        inst_call(QUINE_INST_PAYLOAD) +
        quine_escape(p16(QUINE_PAYLOAD_SIZE)) +
        inst_reverse() +
        inst_call(QUINE_INST_PRINT_ESCAPE_CHAR) +
        inst_push(0) +
        inst_and() +
        inst_and() +
        inst_and() +
        inst_xor() +
        quine_marker_1_ind() +
        inst_emit() +
        inst_push(1)
    )


def quine_print_escape_char():
    return (
        inst_dup() +
        quine_marker_1_ind() +
        inst_xor() +
        inst_condcall(QUINE_INST_ACTUAL_PRINT_ESCAPE_CHAR)
    )


def quine_actual_print_escape_char():
    return (
        inst_dup() +
        inst_push(0) +
        inst_topneg() +
        inst_and() +
        inst_push(2) +
        inst_push(0) +
        inst_reverse() +
        inst_push(0x7f) +
        inst_and() +
        inst_emit() +
        inst_condcall(QUINE_INST_PRINT_ESCAPE_SEQ) +
        inst_call(QUINE_INST_PRINT_ESCAPE_CHAR)
    )


def quine_print_escape_seq():
    return (
        inst_push(0) +
        inst_emit() +
        inst_push(0) +
        inst_topneg() +
        inst_emit()
    )


def quine_escape(code):
    res = ''
    for c in code:
        if ord(c) >= 0x80:
            res += chr(ord(c) & 0x7f) + chr(0x80)
        else:
            res += c
    return res


def quine_define_as(func, name):
    DEBUG_INST_NAMES[name] = func.__name__
    code = func()
    assert len(code) < 0x10000, hexdump(code)
    return (
        quine_escape(code) +
        quine_escape(p16(len(code))) +
        inst_reverse() +
        quine_escape(p16(len(code))) +
        inst_push(name) +
        inst_def()
    )


def quine_begin():
    return (
        quine_define_as(quine_print, QUINE_INST_PRINT) +
        quine_define_as(quine_print_not_escape, QUINE_INST_PRINT_NOT_ESCAPE) +
        quine_define_as(quine_actual_print, QUINE_INST_ACTUAL_PRINT) +
        quine_define_as(quine_print_escape, QUINE_INST_PRINT_ESCAPE) +
        quine_define_as(quine_print_escape_char, QUINE_INST_PRINT_ESCAPE_CHAR) +
        quine_define_as(quine_actual_print_escape_char, QUINE_INST_ACTUAL_PRINT_ESCAPE_CHAR) +
        quine_define_as(quine_print_escape_seq, QUINE_INST_PRINT_ESCAPE_SEQ)
    )


QUINE_PAYLOAD_SIZE = 0xa0
QUINE_ESCAPED_PAYLOAD_SIZE = 0xdc


def quine_end():
    return (
        quine_escape(p16(QUINE_ESCAPED_PAYLOAD_SIZE)) +
        inst_reverse() +
        quine_escape(p16(QUINE_ESCAPED_PAYLOAD_SIZE)) +
        inst_push(QUINE_INST_PAYLOAD) +
        inst_def() +
        inst_call(QUINE_INST_PAYLOAD) +
        quine_escape(p16(QUINE_PAYLOAD_SIZE)) +
        inst_reverse() +
        inst_call(QUINE_INST_PRINT)
    )


def quine_payload():
    payload = (
        quine_begin() +
        inst_push(QUINE_MARKER_2) +
        quine_end() +
        inst_push(QUINE_MARKER_1)
    )
    escaped_payload = quine_escape(payload)
    print("Length of payload: %x" % len(payload))
    print("Length of escaped payload: %x" % len(escaped_payload))
    return quine_escape(escaped_payload)


quine_code = quine_begin() + quine_payload() + quine_end()


# print hexdump(VM(True).Run(
#     quine_begin() +
#     quine_payload() +
#     quine_escape(p16(QUINE_ESCAPED_PAYLOAD_SIZE)) +
#     inst_reverse() +

#     inst_call(QUINE_INST_PRINT)
# ))
# quit()

# print hexdump(VM(True).Run(
#     quine_begin() +
#     inst_push(QUINE_MARKER_1) +
#     inst_push(0x11) +
#     inst_push(0x22) +
#     inst_push(0x33) +
#     inst_push(0x44) +
#     inst_call(QUINE_INST_PRINT)
# ))
# quit()

# vm = VM(False)
# print hexdump(vm.Run(
#   quine_payload() +
#   quine_escape(p16(QUINE_ESCAPED_PAYLOAD_SIZE)) +
#   inst_reverse() +
#   quine_escape(p16(QUINE_ESCAPED_PAYLOAD_SIZE)) +
#   inst_push(QUINE_INST_PAYLOAD) +
#   inst_def() +
#   inst_call(QUINE_INST_PAYLOAD) +
# ))
# quit()


vm = VM(True)
output = vm.Run(
    quine_code
)

print hexdump(quine_end())
print hexdump(quine_code)
print hexdump(output)

print hexdump(xor(quine_code, output))
print quine_code == output
