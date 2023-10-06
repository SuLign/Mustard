using CopyBook.Core;

using Mustard.Base.Toolset;
using Mustard.Interfaces.Framework;
using Mustard.UI.MVVM;

using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CopyBook.ViewModules;

public class AudioIndexViewModule : ViewModelBase
{
    private readonly ReplyCommand playSound;
    private readonly ReplyCommand pause;
    private readonly ReplyCommand resume;
    private AudioPlayer audioPlayer;

    public ReplyCommand PlaySound => playSound;
    public ReplyCommand Pause => pause;
    public ReplyCommand Resume => resume;
    public LazyCommand DisplayMessageBox => new Action(() =>
    {
        SingletonContainer<IMustardMessageManager>.Instance.MessageBoxShow("hello", Mustard.Base.BaseDefinitions.MessageShowType.WARNING);
    });
    public LazyCommand DisplayOpenFileDialog => new Action(() =>
    {
        MustardOpenFileDialog mustardOpenFileDialog = new MustardOpenFileDialog();
        mustardOpenFileDialog.ShowDialog();
    });

    public LazyCommand La => new Action(() =>
    {
        SingletonContainer<IMustardMessageManager>.Instance.MessageBoxShow("hello", Mustard.Base.BaseDefinitions.MessageShowType.WARNING);
    });

    public AudioIndexViewModule()
    {
        audioPlayer = new AudioPlayer();

        playSound = new ReplyCommand(async () =>
        {
            await Task.Run(() =>
            {
                var audioFile = @"E:\PC Folder\Musics\Big Shaq - Man's Not Hot (Dirty Palm Remix).mp3";
                audioPlayer.Play(audioFile);
            });
        });

        pause = new ReplyCommand(() =>
        {
            audioPlayer.Pause();
        });

        resume = new ReplyCommand(() =>
        {
            audioPlayer.Resume();
        });
    }
}
