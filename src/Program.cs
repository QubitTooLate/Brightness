using System;
using Qtl.BrightnessController;
using Qtl.Keylogging.HotKeys;

const int VK_F2 = 0x71;
const int VK_F3 = 0x72;

const int BRIGHTNESS_STEP = 10;
const int MAX_BRIGHNESS = 100;

var brightness = 0;
SetBrightness(brightness);

using var hotKeys = HotKeyTask.StartNew();

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F2, hotKey =>
{
	if (brightness >= BRIGHTNESS_STEP)
	{
		brightness -= BRIGHTNESS_STEP;
		SetBrightness(brightness);
		WriteBrightness(brightness);
	}
});

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F3, hotKey =>
{
	if ((brightness + BRIGHTNESS_STEP) <= MAX_BRIGHNESS)
	{
		brightness += BRIGHTNESS_STEP;
		SetBrightness(brightness);
		WriteBrightness(brightness);
	}
});

Console.ReadKey();

await hotKeys.StopAsync();

return;

static void SetBrightness(int brightness) => MonitorBrightnessController.SetPrimaryMonitorBrightness(brightness / 100.0f);

static void WriteBrightness(int brightness) => Console.WriteLine($"{brightness}% {new string('|', brightness)}");
