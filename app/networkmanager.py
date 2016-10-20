#!/usr/bin/env python2

import openzwave
from openzwave.option import ZWaveOption
from openzwave.node import ZWaveNode
from openzwave.value import ZWaveValue
from openzwave.scene import ZWaveScene
from openzwave.controller import ZWaveController
from openzwave.network import ZWaveNetwork
import libopenzwave
from louie import dispatcher, All
import os
from bulb import *

class NetworkManager:
    def __init__(self, device, config):
        options = ZWaveOption(device, config_path=config, user_path="../cache", cmd_line="")
        options.set_console_output(False)
        options.set_logging(False)
        options.lock()

        self.network = ZWaveNetwork(options, autostart=False)
        dispatcher.connect(self.event_network_started, ZWaveNetwork.SIGNAL_NETWORK_STARTED)
        dispatcher.connect(self.event_network_ready, ZWaveNetwork.SIGNAL_NETWORK_READY)
        dispatcher.connect(self.event_network_failed, ZWaveNetwork.SIGNAL_NETWORK_FAILED)
        self.is_ready = False
        self.network.start()

    def event_network_started(self, network):
        print("Network {} starting.".format(network.home_id))

    def event_network_ready(self, network):
        print("Network is ready. Controller (node {}) at version {}. {} nodes available:".format(network.controller.node.node_id, network.controller.node.version, network.nodes_count))

        for node in network.nodes:
            print("  # Node {}".format(network.nodes[node].node_id))
            print("    - Name:         {}".format(network.nodes[node].name))
            print("    - Manufacturer: {} (id {})".format(network.nodes[node].manufacturer_name, network.nodes[node].manufacturer_id))
            print("    - Product:      {} (id {}, type {})".format(network.nodes[node].product_name, network.nodes[node].product_id, network.nodes[node].product_type))
            print("    - Version:      {}".format(network.nodes[node].version))
            print("    - Commands:     {}".format(network.nodes[node].command_classes_as_string))
            print("    - Capabilities: {}".format(network.nodes[node].capabilities))
            print("    - Neigbors:     {}".format(network.nodes[node].neighbors))
            print("    - Can sleep:    {}".format(network.nodes[node].can_wake_up()))

        self.is_ready = True

    def event_network_failed(self, network):
        print("Network could not load")

    def get_bulbs(self):
        if self.is_ready:
            for node in self.network.nodes:
                if len(self.network.nodes[node].get_dimmers()) > 0 or len(self.network.nodes[node].get_rgbbulbs()) > 0:
                    yield Bulb(self.network.nodes[node])
