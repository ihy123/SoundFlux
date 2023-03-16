netsh advfirewall firewall delete rule name="SoundFlux" dir=in
netsh advfirewall firewall delete rule name="SoundFlux" dir=out

netsh advfirewall firewall add rule name="SoundFlux" dir=in program=%1 action=allow
netsh advfirewall firewall add rule name="SoundFlux" dir=out program=%1 action=allow
