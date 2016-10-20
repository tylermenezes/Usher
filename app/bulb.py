#!/usr/bin/env python2

class Bulb:
    
    def __init__(self, node):
        self.id = node.node_id
        self.node = node
        self.color = (0, 0, 0, 255, 0)
        self.brightness = 100

    # Helper Setters
    def set_rgb(self, r, g, b):
        self.set_rgbl(*self._rgb_to_rgbl_gamut(r, g, b))

    def set_rgb_temperature(self, temperature):
        self.set_rgb(*self._temperature_to_rgb(temperature))

    def set_white(self, lw, lc):
        self.set_rgbl(0, 0, 0, lw, lc)

    def set_white_brightness(self, brightness):
        brightness_mult = (brightness/100.0)*255
        lw, lc = [brightness_mult*x for x in self._get_l_multipliers()]
        self.set_white(lw, lc)

    # Raw Setters
    def set_brightness(self, brightness):
        self.brightness = brightness
        for value in self.node.get_dimmers():
            self.node.set_dimmer(value, int(brightness))

    def set_rgbl(self, r, g, b, lw, lc):
        self.color = (r, g, b, lw, lc)
        for bulb in self.node.get_rgbbulbs():
            self.node.set_rgbw(bulb, '#'+self._dec_to_hex(self.color))

    # Utilities
    def _dec_to_hex(self, dec_tuple):
        return ''.join([hex(int(d)).split('x')[1].zfill(2) for d in dec_tuple])

    def _rgb_to_rgbl_gamut(self, r, g, b):
        mapping = (1.0, 1.0, 1.0)
        if self.node.product_name == 'ZW098 LED Bulb':
            mapping = (1.0, 0.94, 0.17)
        
        r, g, b = [int(a*b) for a,b in zip((r, g, b), mapping)]
        
        return (r, g, b, 0, 0)

    def _temperature_to_rgb(self):
        # Thanks to http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
        temperature = temperature/100

        r = 255
        g = 255
        b = 255

        # Red
        if temperature > 66:
            r = temperature-60
            r = 329.698727446 * math.pow(r,-0.1332047592)

        # Green
        if temperature <= 66:
            g = temperature
            g = 99.4708025861 * math.log(g) - 161.1195681661
        else:
            g = temperature - 60
            g = 288.1221695283 * math.pow(g,-0.0755148492)

        # Blue
        if temperature <= 19:
            b = 0
        elif temperature < 66:
            b = temperature - 10
            b = 138.5177312231 * math.log(b) - 305.0447927307

        # Ceil/Floor
        r, g, b = [int(max(0, min(255, x))) for x in (r, g, b)]

        return (r, g, b)

    def _get_l_multipliers(self):
        lw = self.color[3]
        lc = self.color[4]

        lw_rel = 0.0
        lc_rel = 0.0
        
        if lw == 0 and lc == 0:
            lw_rel = 1.0
            lc_rel = 1.0
        elif lw >= lc:
            lw_rel = 1.0
            lc_rel = lc/float(lw)
        else:
            lw_rel = lw/float(lc)
            lc_rel = 1.0

        return (lw_rel, lc_rel)
