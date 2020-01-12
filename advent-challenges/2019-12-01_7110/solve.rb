#!/usr/bin/env ruby

def die(msg)
  puts msg
  exit(1)
end

die("usage: #{$PROGRAM_NAME} <keylog.csv>") unless ARGV.length == 1
keylog = File.open(ARGV[0], 'r') { |f| f.readlines.map(&:chomp) }

KEYS = {
  0 => ' 0',
  1 => '.,\'?!"1-()@/:',
  2 => 'abc2',
  3 => 'def3',
  4 => 'ghi4',
  5 => 'jkl5',
  6 => 'mno6',
  7 => 'pqrs7',
  8 => 'tuv8',
  9 => 'wxyz9',
  10 => '@/:_;+&%*[]{}',
  11 => :hash,
  100 => :left,
  101 => :right,
  102 => :up,
  103 => :down,
  104 => :accept,
  105 => :reject
}

$last_length = nil

def report_progress(string)
  # puts string
  print " #{' ' * $last_length}\r" if $last_length
  print " #{string}\r"
  $last_length = string.length
  sleep(0.01)
end

$buffer = ''
$cursor = 0

def buffer_insert!(char)
  before = $buffer[0...$cursor] || ''
  after = $buffer[$cursor..-1] || ''
  $buffer = before + char + after
  $cursor += 1
end

def buffer_change!(char)
  before = $buffer[0...$cursor-1] || ''
  after = $buffer[$cursor..-1] || ''
  $buffer = before + char + after
end

def buffer_erase!
  before = $buffer[0...$cursor-1] || ''
  after = $buffer[$cursor..-1] || ''
  $buffer = before + after
  $cursor = [$cursor - 1, 0].max
end

def buffer_move!(amount)
  cursor = $cursor + amount
  $cursor = [cursor, 0].max
  $cursor = [cursor, $buffer.length].min
end

long_press = 1000
buffer = ''
cursor = 0
last_key = 0
last_index = 0
last_timestamp = 0
keylog.each do |line|
  timestamp, key = line.chomp.split(',').map(&:to_i)
  value = KEYS[key] || die("unknown key #{key}")
  case value
  when String
    if key != last_key || timestamp - last_timestamp >= long_press
      last_index = 0
      buffer_insert!(value[last_index])
    else
      last_index = (last_index + 1) % value.length
      buffer_change!(value[last_index])
    end
  when :right then buffer_erase!
  when :up then buffer_move!(-1)
  when :down then buffer_move!(1)
  end
  report_progress($buffer)
  last_key = key
  last_timestamp = timestamp
end
puts
