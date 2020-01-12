#!/usr/bin/env ruby
USAGE = "usage: #{$PROGRAM_NAME} <in.bin>"

TILE_MAPPING_ARRAY = [
  ['Q', '?', '0', '\\', 'H', '$', 'Y', ','],
  ['R', :box_hor, 'L', '^', 'K', 'J', :box_bot_r, 'k'],
  ['s', '#', '_', '/', 'm', '=', 'f', '9'],
  ['7', 'd', '-', 'N', 'E', '4', 'q', 'r'],
  ['P', 'i', :box_bot_l, 'V', '`', '&', 'X', 'A'],
  ['n', '3', 'I', :box_top_l, 'O', '*', ';', 'Z'],
  ['w', 'G', 'p', 'B', '8', 'c', 'S', 'j'],
  ['F', 'g', ':', 'e', 'b', 'y', '"', 'v'],
  ['%', '+', :box_top_r, '1', ' ', '!', 'M', '@'],
  ['h', '{', '2', 'x', 'W', '.', 'D', '}'],
  ['t', 'U', :box_ver, 'C', 'T', 'z', '6', 'u'],
  ['|', 'o', '>', 'a', '5', 'l', '<', "'"]
]

def die(msg)
  puts msg
  exit(1)
end

def assert(condition)
  raise('assertion failed') unless condition
end

def read_str(io, n)
  io.read(n)
end

def read_u8(io)
  io.getbyte
end

def read_u16(io)
  io.read(2).unpack('s>')[0]
end

def read_u32(io)
  io.read(4).unpack('l>')[0]
end

die(USAGE) unless ARGV.length == 1
binfile = ARGV[0]

File.open(binfile, 'rb') do |bin|
  assert(read_str(bin, 4) == 'TiNy')
  assert(read_str(bin, 4) == 'MeTa')
  assert(read_u32(bin) == 16)
  assert(read_str(bin, 3) == "\x00\x00\x00")
  puts "Palette colors: #{read_u8(bin)}"
  puts "Tile width: #{read_u16(bin)}"
  puts "Tile height: #{read_u16(bin)}"
  puts "Tile columns: #{read_u8(bin)}"
  puts "Tile rows: #{read_u8(bin)}"
  puts "Characters: #{read_u16(bin)}"
  lines = read_u16(bin)
  puts "Lines: #{lines}"
  puts "Max line length: #{read_u16(bin)}"
  assert(read_str(bin, 4) == 'TxTr')
  texture_length = read_u32(bin)
  read_str(bin, texture_length)
  lines.times do
    assert(read_str(bin, 4) == 'LiNe')
    line_length = read_u32(bin) / 2
    puts "Line length: #{line_length}"
    line_length.times do
      x = read_u8(bin)
      y = read_u8(bin)
      char = TILE_MAPPING_ARRAY[y][x]
      die("unknown char #{char.inspect}") unless char
      STDOUT.write(char)
    end
    puts
  end
end
