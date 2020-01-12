#!/usr/bin/env ruby
USAGE = "usage: #{$PROGRAM_NAME} <tiles.png> <in.txt> <out.bin>"

TILE_SIZE = 8
TILE_WIDTH = 8
TILE_HEIGHT = 12
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

TILE_MAPPING_HASH = {}
TILE_MAPPING_ARRAY.each_with_index do |row, y|
  row.each_with_index do |col, x|
    TILE_MAPPING_HASH[col] = [x, y]
  end
end

def die(msg)
  puts msg
  exit(1)
end

def write_str(io, s)
  io.write(s)
end

def write_u8(io, n)
  io.putc(n)
end

def write_u16(io, n)
  io.write([n].pack('s>'))
end

def write_u32(io, n)
  io.write([n].pack('l>'))
end

die(USAGE) unless ARGV.length == 3
tilefile, txtfile, binfile = ARGV
lines = File.open(txtfile, 'r', &:readlines).map(&:chomp)
tilepng = File.open(tilefile, 'rb', &:read)

File.open(binfile, 'wb') do |file|
  write_str(file, 'TiNy')
  write_str(file, 'MeTa')
  write_u32(file, 16)
  write_str(file, "\x00\x00\x00")
  write_u8(file, 2)
  write_u16(file, TILE_SIZE)
  write_u16(file, TILE_SIZE)
  write_u8(file, TILE_WIDTH)
  write_u8(file, TILE_HEIGHT)
  write_u16(file, lines.map(&:length).reduce(0, &:+))
  write_u16(file, lines.length)
  write_u16(file, lines.map(&:length).max)
  write_str(file, 'TxTr')
  write_u32(file, tilepng.length)
  write_str(file, tilepng)
  lines.each do |line|
    write_str(file, 'LiNe')
    write_u32(file, line.length * 2)
    line.each_char do |char|
      x, y = TILE_MAPPING_HASH[char]
      die("unknown char: #{char.inspect}") unless x && y
      write_u8(file, x)
      write_u8(file, y)
    end
  end
end
