using System.Threading;
using Qtl.BrightnessController;
using Qtl.Keylogging.HotKeys;
using Qtl.RawWacom;

const int VK_F2 = 0x71;
const int VK_F3 = 0x72;
const int VK_F4 = 0x73;

const int BRIGHTNESS_STEP = 10;
const int MAX_BRIGHNESS = 100;

using var singletonMutex = new Mutex(true, "F8C61D2A-BAA9-43E3-818A-5D27863DC260", out var isSingleInstance);
if (!isSingleInstance)
{
	return;
}

using var processInformation = new ProcessInformation();
_ = processInformation.SetEfficiencyMode();

var brightness = 0;
SetBrightness(brightness);

using var hotKeys = HotKeyTask.StartNew();
using var exitApplicationEvent = new ManualResetEvent(false);

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F2, hotKey =>
{
	if (brightness >= BRIGHTNESS_STEP)
	{
		brightness -= BRIGHTNESS_STEP;
		SetBrightness(brightness);
	}
});

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F3, hotKey =>
{
	if ((brightness + BRIGHTNESS_STEP) <= MAX_BRIGHNESS)
	{
		brightness += BRIGHTNESS_STEP;
		SetBrightness(brightness);
	}
});

hotKeys.AddHotKey(HotKeyModifiers.Win, VK_F4, hotKey =>
{
	_ = exitApplicationEvent.Set();
});

_ = exitApplicationEvent.WaitOne();

await hotKeys.StopAsync();

return;

static void SetBrightness(int brightness) => MonitorBrightnessController.SetPrimaryMonitorBrightness(brightness / 100.0f);
