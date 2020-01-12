#!/usr/bin/env python2
# -*- coding: utf-8 -*-
##################################################
# GNU Radio Python Flow Graph
# Title: Rds Tx
# Generated: Sun Nov 17 16:38:28 2019
##################################################


if __name__ == '__main__':
    import ctypes
    import sys
    if sys.platform.startswith('linux'):
        try:
            x11 = ctypes.cdll.LoadLibrary('libX11.so')
            x11.XInitThreads()
        except:
            print "Warning: failed to XInitThreads()"

from gnuradio import analog
from gnuradio import audio
from gnuradio import blocks
from gnuradio import digital
from gnuradio import eng_notation
from gnuradio import filter
from gnuradio import gr
from gnuradio import wxgui
from gnuradio.eng_option import eng_option
from gnuradio.fft import window
from gnuradio.filter import firdes
from gnuradio.wxgui import fftsink2
from gnuradio.wxgui import forms
from grc_gnuradio import wxgui as grc_wxgui
from optparse import OptionParser
import math
import rds
import wx


class rds_tx(grc_wxgui.top_block_gui):

    def __init__(self):
        grc_wxgui.top_block_gui.__init__(self, title="Rds Tx")
        _icon_path = "/usr/share/icons/hicolor/32x32/apps/gnuradio-grc.png"
        self.SetIcon(wx.Icon(_icon_path, wx.BITMAP_TYPE_ANY))

        ##################################################
        # Variables
        ##################################################
        self.usrp_rate = usrp_rate = 19e3*20
        self.stereo_gain = stereo_gain = .3
        self.rt = rt = ""
        self.rds_gain = rds_gain = .27
        self.ps = ps = "Part 3/3"
        self.pilot_gain = pilot_gain = .09
        self.outbuffer = outbuffer = 10
        self.input_gain = input_gain = .3
        self.freq = freq = 87.5e6
        self.fm_max_dev = fm_max_dev = 80e3

        ##################################################
        # Blocks
        ##################################################
        self._rt_text_box = forms.text_box(
        	parent=self.GetWin(),
        	value=self.rt,
        	callback=self.set_rt,
        	label='RadioText',
        	converter=forms.str_converter(),
        )
        self.Add(self._rt_text_box)
        _rds_gain_sizer = wx.BoxSizer(wx.VERTICAL)
        self._rds_gain_text_box = forms.text_box(
        	parent=self.GetWin(),
        	sizer=_rds_gain_sizer,
        	value=self.rds_gain,
        	callback=self.set_rds_gain,
        	label='rds_gain',
        	converter=forms.float_converter(),
        	proportion=0,
        )
        self._rds_gain_slider = forms.slider(
        	parent=self.GetWin(),
        	sizer=_rds_gain_sizer,
        	value=self.rds_gain,
        	callback=self.set_rds_gain,
        	minimum=0,
        	maximum=3,
        	num_steps=100,
        	style=wx.SL_HORIZONTAL,
        	cast=float,
        	proportion=1,
        )
        self.Add(_rds_gain_sizer)
        self._ps_text_box = forms.text_box(
        	parent=self.GetWin(),
        	value=self.ps,
        	callback=self.set_ps,
        	label='PS',
        	converter=forms.str_converter(),
        )
        self.Add(self._ps_text_box)
        _pilot_gain_sizer = wx.BoxSizer(wx.VERTICAL)
        self._pilot_gain_text_box = forms.text_box(
        	parent=self.GetWin(),
        	sizer=_pilot_gain_sizer,
        	value=self.pilot_gain,
        	callback=self.set_pilot_gain,
        	label='pilot_gain',
        	converter=forms.float_converter(),
        	proportion=0,
        )
        self._pilot_gain_slider = forms.slider(
        	parent=self.GetWin(),
        	sizer=_pilot_gain_sizer,
        	value=self.pilot_gain,
        	callback=self.set_pilot_gain,
        	minimum=0,
        	maximum=3,
        	num_steps=100,
        	style=wx.SL_HORIZONTAL,
        	cast=float,
        	proportion=1,
        )
        self.Add(_pilot_gain_sizer)
        _input_gain_sizer = wx.BoxSizer(wx.VERTICAL)
        self._input_gain_text_box = forms.text_box(
        	parent=self.GetWin(),
        	sizer=_input_gain_sizer,
        	value=self.input_gain,
        	callback=self.set_input_gain,
        	label='input_gain',
        	converter=forms.float_converter(),
        	proportion=0,
        )
        self._input_gain_slider = forms.slider(
        	parent=self.GetWin(),
        	sizer=_input_gain_sizer,
        	value=self.input_gain,
        	callback=self.set_input_gain,
        	minimum=0,
        	maximum=10,
        	num_steps=100,
        	style=wx.SL_HORIZONTAL,
        	cast=float,
        	proportion=1,
        )
        self.Add(_input_gain_sizer)
        self.wxgui_fftsink2_0 = fftsink2.fft_sink_f(
        	self.GetWin(),
        	baseband_freq=0,
        	y_per_div=20,
        	y_divs=10,
        	ref_level=0,
        	ref_scale=2.0,
        	sample_rate=usrp_rate,
        	fft_size=1024,
        	fft_rate=30,
        	average=False,
        	avg_alpha=None,
        	title='FFT Plot',
        	peak_hold=False,
        )
        self.Add(self.wxgui_fftsink2_0.win)
        _stereo_gain_sizer = wx.BoxSizer(wx.VERTICAL)
        self._stereo_gain_text_box = forms.text_box(
        	parent=self.GetWin(),
        	sizer=_stereo_gain_sizer,
        	value=self.stereo_gain,
        	callback=self.set_stereo_gain,
        	label='stereo_gain',
        	converter=forms.float_converter(),
        	proportion=0,
        )
        self._stereo_gain_slider = forms.slider(
        	parent=self.GetWin(),
        	sizer=_stereo_gain_sizer,
        	value=self.stereo_gain,
        	callback=self.set_stereo_gain,
        	minimum=0,
        	maximum=3,
        	num_steps=100,
        	style=wx.SL_HORIZONTAL,
        	cast=float,
        	proportion=1,
        )
        self.Add(_stereo_gain_sizer)
        self.rational_resampler_xxx_1 = filter.rational_resampler_ccc(
                interpolation=1000,
                decimation=380,
                taps=None,
                fractional_bw=None,
        )
        self.rational_resampler_xxx_0_0 = filter.rational_resampler_fff(
                interpolation=380,
                decimation=16,
                taps=None,
                fractional_bw=None,
        )
        self.rational_resampler_xxx_0 = filter.rational_resampler_fff(
                interpolation=380,
                decimation=16,
                taps=None,
                fractional_bw=None,
        )
        self.low_pass_filter_0_0_0 = filter.interp_fir_filter_fff(1, firdes.low_pass(
        	1, usrp_rate, 15e3, 2e3, firdes.WIN_HAMMING, 6.76))
        self.low_pass_filter_0_0 = filter.interp_fir_filter_fff(1, firdes.low_pass(
        	1, usrp_rate, 15e3, 2e3, firdes.WIN_HAMMING, 6.76))
        self.low_pass_filter_0 = filter.interp_fir_filter_fff(1, firdes.low_pass(
        	1, usrp_rate, 2.5e3, .5e3, firdes.WIN_HAMMING, 6.76))
        (self.low_pass_filter_0).set_max_output_buffer(10)
        self.gr_unpack_k_bits_bb_0 = blocks.unpack_k_bits_bb(2)
        (self.gr_unpack_k_bits_bb_0).set_max_output_buffer(10)
        self.gr_sub_xx_0 = blocks.sub_ff(1)
        self.gr_sig_source_x_0_1 = analog.sig_source_f(usrp_rate, analog.GR_SIN_WAVE, 19e3, 1, 0)
        self.gr_sig_source_x_0_0 = analog.sig_source_f(usrp_rate, analog.GR_SIN_WAVE, 57e3, 1, 0)
        self.gr_sig_source_x_0 = analog.sig_source_f(usrp_rate, analog.GR_SIN_WAVE, 38e3, 1, 0)
        self.gr_rds_encoder_0 = rds.encoder(0, 14, True, ps, 89.8e6,
        			True, False, 6, 1,
        			147, rt)

        (self.gr_rds_encoder_0).set_max_output_buffer(10)
        self.gr_multiply_xx_1 = blocks.multiply_vff(1)
        self.gr_multiply_xx_0 = blocks.multiply_vff(1)
        (self.gr_multiply_xx_0).set_max_output_buffer(10)
        self.gr_map_bb_1 = digital.map_bb(([1,2]))
        (self.gr_map_bb_1).set_max_output_buffer(10)
        self.gr_map_bb_0 = digital.map_bb(([-1,1]))
        (self.gr_map_bb_0).set_max_output_buffer(10)
        self.gr_frequency_modulator_fc_0 = analog.frequency_modulator_fc(2*math.pi*fm_max_dev/usrp_rate)
        (self.gr_frequency_modulator_fc_0).set_max_output_buffer(10)
        self.gr_diff_encoder_bb_0 = digital.diff_encoder_bb(2)
        (self.gr_diff_encoder_bb_0).set_max_output_buffer(10)
        self.gr_char_to_float_0 = blocks.char_to_float(1, 1)
        (self.gr_char_to_float_0).set_max_output_buffer(10)
        self.gr_add_xx_1 = blocks.add_vff(1)
        (self.gr_add_xx_1).set_max_output_buffer(10)
        self.gr_add_xx_0 = blocks.add_vff(1)
        self.blocks_wavfile_source_0 = blocks.wavfile_source('/home/deepstar/Projects/OverTheWire/advent2019/advent2019/steven/santa-radio/part3/part3-new.wav', False)
        self.blocks_socket_pdu_0 = blocks.socket_pdu("TCP_SERVER", '', '52001', 10000, False)
        self.blocks_repeat_0 = blocks.repeat(gr.sizeof_float*1, 160)
        self.blocks_multiply_const_vxx_0_1 = blocks.multiply_const_vff((input_gain, ))
        self.blocks_multiply_const_vxx_0_0_1 = blocks.multiply_const_vff((pilot_gain, ))
        self.blocks_multiply_const_vxx_0_0 = blocks.multiply_const_vff((rds_gain, ))
        (self.blocks_multiply_const_vxx_0_0).set_max_output_buffer(10)
        self.blocks_multiply_const_vxx_0 = blocks.multiply_const_vff((input_gain, ))
        self.blocks_file_sink_0 = blocks.file_sink(gr.sizeof_gr_complex*1, '/home/deepstar/Projects/OverTheWire/advent2019/advent2019/steven/santa-radio/part3/fm3-small.wav', False)
        self.blocks_file_sink_0.set_unbuffered(False)
        self.audio_sink_0 = audio.sink(16000, '', True)

        ##################################################
        # Connections
        ##################################################
        self.msg_connect((self.blocks_socket_pdu_0, 'pdus'), (self.gr_rds_encoder_0, 'rds in'))
        self.connect((self.blocks_multiply_const_vxx_0, 0), (self.rational_resampler_xxx_0, 0))
        self.connect((self.blocks_multiply_const_vxx_0_0, 0), (self.gr_add_xx_1, 0))
        self.connect((self.blocks_multiply_const_vxx_0_0_1, 0), (self.gr_add_xx_1, 1))
        self.connect((self.blocks_multiply_const_vxx_0_1, 0), (self.rational_resampler_xxx_0_0, 0))
        self.connect((self.blocks_repeat_0, 0), (self.low_pass_filter_0, 0))
        self.connect((self.blocks_wavfile_source_0, 0), (self.audio_sink_0, 0))
        self.connect((self.blocks_wavfile_source_0, 0), (self.blocks_multiply_const_vxx_0, 0))
        self.connect((self.blocks_wavfile_source_0, 0), (self.blocks_multiply_const_vxx_0_1, 0))
        self.connect((self.gr_add_xx_0, 0), (self.low_pass_filter_0_0, 0))
        self.connect((self.gr_add_xx_1, 0), (self.gr_frequency_modulator_fc_0, 0))
        self.connect((self.gr_add_xx_1, 0), (self.wxgui_fftsink2_0, 0))
        self.connect((self.gr_char_to_float_0, 0), (self.blocks_repeat_0, 0))
        self.connect((self.gr_diff_encoder_bb_0, 0), (self.gr_map_bb_1, 0))
        self.connect((self.gr_frequency_modulator_fc_0, 0), (self.rational_resampler_xxx_1, 0))
        self.connect((self.gr_map_bb_0, 0), (self.gr_char_to_float_0, 0))
        self.connect((self.gr_map_bb_1, 0), (self.gr_unpack_k_bits_bb_0, 0))
        self.connect((self.gr_multiply_xx_0, 0), (self.blocks_multiply_const_vxx_0_0, 0))
        self.connect((self.gr_multiply_xx_1, 0), (self.gr_add_xx_1, 2))
        self.connect((self.gr_rds_encoder_0, 0), (self.gr_diff_encoder_bb_0, 0))
        self.connect((self.gr_sig_source_x_0, 0), (self.gr_multiply_xx_1, 0))
        self.connect((self.gr_sig_source_x_0_0, 0), (self.gr_multiply_xx_0, 0))
        self.connect((self.gr_sig_source_x_0_1, 0), (self.blocks_multiply_const_vxx_0_0_1, 0))
        self.connect((self.gr_sub_xx_0, 0), (self.low_pass_filter_0_0_0, 0))
        self.connect((self.gr_unpack_k_bits_bb_0, 0), (self.gr_map_bb_0, 0))
        self.connect((self.low_pass_filter_0, 0), (self.gr_multiply_xx_0, 1))
        self.connect((self.low_pass_filter_0_0, 0), (self.gr_add_xx_1, 3))
        self.connect((self.low_pass_filter_0_0_0, 0), (self.gr_multiply_xx_1, 1))
        self.connect((self.rational_resampler_xxx_0, 0), (self.gr_add_xx_0, 0))
        self.connect((self.rational_resampler_xxx_0, 0), (self.gr_sub_xx_0, 0))
        self.connect((self.rational_resampler_xxx_0_0, 0), (self.gr_add_xx_0, 1))
        self.connect((self.rational_resampler_xxx_0_0, 0), (self.gr_sub_xx_0, 1))
        self.connect((self.rational_resampler_xxx_1, 0), (self.blocks_file_sink_0, 0))

    def get_usrp_rate(self):
        return self.usrp_rate

    def set_usrp_rate(self, usrp_rate):
        self.usrp_rate = usrp_rate
        self.wxgui_fftsink2_0.set_sample_rate(self.usrp_rate)
        self.low_pass_filter_0_0_0.set_taps(firdes.low_pass(1, self.usrp_rate, 15e3, 2e3, firdes.WIN_HAMMING, 6.76))
        self.low_pass_filter_0_0.set_taps(firdes.low_pass(1, self.usrp_rate, 15e3, 2e3, firdes.WIN_HAMMING, 6.76))
        self.low_pass_filter_0.set_taps(firdes.low_pass(1, self.usrp_rate, 2.5e3, .5e3, firdes.WIN_HAMMING, 6.76))
        self.gr_sig_source_x_0_1.set_sampling_freq(self.usrp_rate)
        self.gr_sig_source_x_0_0.set_sampling_freq(self.usrp_rate)
        self.gr_sig_source_x_0.set_sampling_freq(self.usrp_rate)
        self.gr_frequency_modulator_fc_0.set_sensitivity(2*math.pi*self.fm_max_dev/self.usrp_rate)

    def get_stereo_gain(self):
        return self.stereo_gain

    def set_stereo_gain(self, stereo_gain):
        self.stereo_gain = stereo_gain
        self._stereo_gain_slider.set_value(self.stereo_gain)
        self._stereo_gain_text_box.set_value(self.stereo_gain)

    def get_rt(self):
        return self.rt

    def set_rt(self, rt):
        self.rt = rt
        self._rt_text_box.set_value(self.rt)

    def get_rds_gain(self):
        return self.rds_gain

    def set_rds_gain(self, rds_gain):
        self.rds_gain = rds_gain
        self._rds_gain_slider.set_value(self.rds_gain)
        self._rds_gain_text_box.set_value(self.rds_gain)
        self.blocks_multiply_const_vxx_0_0.set_k((self.rds_gain, ))

    def get_ps(self):
        return self.ps

    def set_ps(self, ps):
        self.ps = ps
        self._ps_text_box.set_value(self.ps)
        self.gr_rds_encoder_0.set_ps(self.ps)

    def get_pilot_gain(self):
        return self.pilot_gain

    def set_pilot_gain(self, pilot_gain):
        self.pilot_gain = pilot_gain
        self._pilot_gain_slider.set_value(self.pilot_gain)
        self._pilot_gain_text_box.set_value(self.pilot_gain)
        self.blocks_multiply_const_vxx_0_0_1.set_k((self.pilot_gain, ))

    def get_outbuffer(self):
        return self.outbuffer

    def set_outbuffer(self, outbuffer):
        self.outbuffer = outbuffer

    def get_input_gain(self):
        return self.input_gain

    def set_input_gain(self, input_gain):
        self.input_gain = input_gain
        self._input_gain_slider.set_value(self.input_gain)
        self._input_gain_text_box.set_value(self.input_gain)
        self.blocks_multiply_const_vxx_0_1.set_k((self.input_gain, ))
        self.blocks_multiply_const_vxx_0.set_k((self.input_gain, ))

    def get_freq(self):
        return self.freq

    def set_freq(self, freq):
        self.freq = freq

    def get_fm_max_dev(self):
        return self.fm_max_dev

    def set_fm_max_dev(self, fm_max_dev):
        self.fm_max_dev = fm_max_dev
        self.gr_frequency_modulator_fc_0.set_sensitivity(2*math.pi*self.fm_max_dev/self.usrp_rate)


def main(top_block_cls=rds_tx, options=None):

    tb = top_block_cls()
    tb.Start(True)
    tb.Wait()


if __name__ == '__main__':
    main()
