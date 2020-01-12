#!/usr/bin/env ruby
require 'chunky_png'

TILE_SIZE = 8

def die(msg)
  puts msg
  exit(1)
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

die("usage: #{$PROGRAM_NAME} <in.png> <out.png>") unless ARGV.length == 2
infile, outfile = ARGV
inpng = ChunkyPNG::Image.from_file(infile)
width = inpng.dimension.width
height = inpng.dimension.height
tile_width = width / TILE_SIZE
tile_height = height / TILE_SIZE
grid = (0...tile_height).to_a.product((0...tile_width).to_a)
random_grid = grid.shuffle.each_slice(tile_width).to_a

outpng = ChunkyPNG::Image.new(width, height)
random_grid.each_with_index do |row, r|
  row.each_with_index do |rc, c|
    copy_rect([inpng, rc[1] * TILE_SIZE, rc[0] * TILE_SIZE],
              [outpng, c * TILE_SIZE, r * TILE_SIZE],
              TILE_SIZE, TILE_SIZE)
  end
end

File.open(outfile, 'wb') { |f| outpng.write(f, :best_compression) }
