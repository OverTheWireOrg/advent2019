#!/usr/bin/env ruby
require 'chunky_png'

USAGE = "usage: #{$PROGRAM_NAME} <in.bin> <out.png>"

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

def copy_rect(from, to, width, height)
  from_png, from_x, from_y = from
  to_png, to_x, to_y = to
  height.times do |y|
    width.times do |x|
      pixel = from_png.get_pixel(from_x + x, from_y + y)
      to_png.set_pixel(to_x + x, to_y + y, pixel)
    end
  end
end

die(USAGE) unless ARGV.length == 2
binfile, pngfile = ARGV

File.open(binfile, 'rb') do |bin|
  assert(read_str(bin, 4) == 'TiNy')
  assert(read_str(bin, 4) == 'MeTa')
  assert(read_u32(bin) == 16)
  assert(read_str(bin, 3) == "\x00\x00\x00")
  assert(read_u8(bin) == 2)
  tile_width = read_u16(bin)
  tile_height = read_u16(bin)
  _tile_columns = read_u8(bin)
  _tile_rows = read_u8(bin)
  _char_count = read_u16(bin)
  lines = read_u16(bin)
  max_line_length = read_u16(bin)
  png_width = tile_width * max_line_length
  png_height = tile_height * lines
  assert(read_str(bin, 4) == 'TxTr')
  texture_length = read_u32(bin)
  texture = read_str(bin, texture_length)
  tiles = ChunkyPNG::Image.from_blob(texture)
  png = ChunkyPNG::Image.new(png_width, png_height, ChunkyPNG::Color::BLACK)
  (0...lines).each do |row|
    assert(read_str(bin, 4) == 'LiNe')
    line_length = read_u32(bin) / 2
    (0...line_length).each do |col|
      xo = read_u8(bin)
      yo = read_u8(bin)
      copy_rect([tiles, xo * tile_width, yo * tile_height],
                [png, col * tile_width, row * tile_height],
                tile_width, tile_height)
    end
  end
  File.open(pngfile, 'wb') { |f| png.write(f, :best_compression) }
end
