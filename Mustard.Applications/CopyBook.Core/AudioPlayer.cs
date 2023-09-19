using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;

using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyBook.Core;

public class AudioPlayer : IDisposable
{
    private WaveOutEvent waveOutEvent;

    public AudioPlayer()
    {
        waveOutEvent = new WaveOutEvent();
    }

    public bool Play(string filePath)
    {
        try
        {
            if (waveOutEvent.PlaybackState == PlaybackState.Playing || waveOutEvent.PlaybackState == PlaybackState.Paused)
            {
                waveOutEvent.Stop();
            }
            using (var audiofileReader = new AudioFileReader(filePath))
            {
                waveOutEvent.Init(audiofileReader);
                waveOutEvent.Play();
            }
        }
        catch (Exception ex)
        {
            SingletonContainer<IMustardMessageManager>.Instance.Log(ex.Message);
            return false;
        }
        return true;
    }

    public void Pause()
    {
        if (waveOutEvent.PlaybackState == PlaybackState.Playing)
        {
            waveOutEvent.Pause();
        }
    }

    public void Resume()
    {
        if (waveOutEvent.PlaybackState == PlaybackState.Paused)
        {
            waveOutEvent.Play();
        }
    }

    public void Dispose()
    {
        waveOutEvent.Dispose();
    }
}
