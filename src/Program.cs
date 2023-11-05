using System;
using Qtl.BrightnessController;
using Qtl.Keylogging.HotKeys;

const int VK_F2 = 0x71;
const int VK_F3 = 0x72;

const float BRIGHTNESS_STEP = 0.1f;

MonitorBrightnessController.SetPrimaryMonitorBrightness(0.0f);

using var hotKeys = HotKeyTask.StartNew();

var brightness = 0.0f;

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F2, hotKey =>
{
	if (brightness >= BRIGHTNESS_STEP)
	{
		brightness -= BRIGHTNESS_STEP;
		MonitorBrightnessController.SetPrimaryMonitorBrightness(brightness);
		WriteBrightness(brightness);
	}
});

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F3, hotKey =>
{
	if (brightness <= 1.0f)
	{
		brightness += BRIGHTNESS_STEP;
		MonitorBrightnessController.SetPrimaryMonitorBrightness(brightness);
		WriteBrightness(brightness);
	}
});

Console.ReadKey();

await hotKeys.StopAsync();

return;

static void WriteBrightness(float brightness) => Console.WriteLine($"{brightness:P1} {new string('|', (int)(brightness * 100.0f))}");
