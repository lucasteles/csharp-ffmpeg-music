﻿using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Seconds = System.Single; // System.Single is float
using Sample = System.Single;
using Pulse = System.Single;
using Hz = System.Single;
using Semitons = System.Single;
using Beats = System.Single;
using OpenTK.Audio.OpenAL;
using System.Threading;

var sampleRate = 48000f;
var pitchStandard = 440f;
var volume = .5f;
var bpm = 120f;
var beatsPerSecond = 60f / bpm;

void Play(Sample[][] wave)
{
    var waveArray = wave.SelectMany(x => x).ToArray();
    var deviceName = ALC.GetString (ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
    var device = ALC.OpenDevice(deviceName);
    var context = ALC.CreateContext(device, (int[])null);
    ALC.MakeContextCurrent(context);
    AL.GenBuffer(out var alBuffer);
    AL.BufferData(alBuffer, ALFormat.MonoFloat32Ext, waveArray, (int)sampleRate);

    AL.Listener(ALListenerf.Gain, 1f);
    AL.GenSource(out var alSource);
    AL.Source(alSource, ALSourcef.Gain, 1f);
    AL.Source(alSource, ALSourcei.Buffer, alBuffer);
    AL.SourcePlay(alSource);
    while (AL.GetSourceState(alSource) == ALSourceState.Playing)
        Thread.Sleep(10);
    AL.SourceStop(alSource);
    ALC.MakeContextCurrent(ALContext.Null);
    ALC.DestroyContext(context);
    ALC.CloseDevice(device);
}

Pulse[] GetWave(float step, Seconds duration) =>
    Enumerable
        .Range(0, (int) (sampleRate * duration))
        .Select(x => x * step)
        .Select(MathF.Sin)
        .Select(x => x * volume)
        .ToArray();

Pulse[] Freq(Hz hz, Seconds duration)
{
    var step = hz * 2 * MathF.PI / sampleRate;
    var output = GetWave(step, duration);

    var attack =
        Enumerable.Range(0, output.Length)
            .Select(x => MathF.Min(1, x / 1000f));
    var release = attack.Reverse();
    var wave = output
        .Zip(attack, (w, v) => w * v)
        .Zip(release, (w, v) => w * v)
        .ToArray();

    return wave;
}

Hz F(Semitons n) => (float) (pitchStandard * Math.Pow(Math.Pow(2, 1.0 / 12.0), n));
Pulse[] NoteFreq(Semitons n, Beats beats) => Freq(F(n), (beats * beatsPerSecond));
Pulse[][] Cycle(Pulse[][] list, int n) => Enumerable.Range(0, n).SelectMany(_ => list).ToArray();

Pulse[] Note(N note, Beats beats,  int level = 0)
{
    var pos = ((int)note - (int)N.A) + level * Enum.GetNames<N>().Length;
    return NoteFreq(pos, beats);
}
// from https://www.youtube.com/watch?v=FtWIuFLBrjo
var intro = new[]
{
    Note(N.A,  0.5f),
    Note(N.A,  0.5f),
    Note(N.A,  0.5f),
    Note(N.A,  0.5f),

    Note(N.Cs,  0.5f, 1),
    Note(N.Cs,  0.5f, 1),
    Note(N.Cs,  0.5f, 1),
    Note(N.Cs,  0.5f, 1),

    Note(N.B,  0.5f),
    Note(N.B,  0.5f),
    Note(N.B,  0.5f),
    Note(N.B,  0.5f),

    Note(N.M,  0.5f, 1),
    Note(N.M,  0.5f, 1),
    Note(N.M,  0.5f, 1),
    Note(N.M,  0.5f, 1),

    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),

    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),

    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),
    Note(N.Fs, 0.5f,1 ),

    Note(N.B, 0.5f ),
    Note(N.A, 0.5f ),
    Note(N.Gs, 0.5f ),
    Note(N.M, 0.5f ),
};

var verso = new[]
{
    Note(N.Fs, 1f),
    Note(N.Fs, 0.5f),
    Note(N.Cs, 0.5f,1),
    Note(N.B, 1f),
    Note(N.A, 1f),
    Note(N.Gs, 1f ),
    Note(N.Gs, 0.5f ),
    Note(N.Gs, 0.5f ),
    Note(N.B, 1),
    Note(N.A, 0.5f),
    Note(N.Gs, 0.5f ),
    Note(N.Fs, 1f ),

    Note(N.Fs, 0.5f ),
    Note(N.A, 0.5f,1),
    Note(N.Gs, 0.5f,1),
    Note(N.A, 0.5f,1),
    Note(N.Gs, 0.5f,1),
    Note(N.A, 0.5f,1),
    Note(N.Fs, 1f ),

    Note(N.Fs, 0.5f ),
    Note(N.A, 0.5f,1),
    Note(N.Gs, 0.5f,1),
    Note(N.A, 0.5f,1),
    Note(N.Gs, 0.5f,1),
    Note(N.A, 0.5f,1),
};

var doisVersos = Cycle(verso, 2);
var halfMusic = intro.Concat(doisVersos).ToArray();
var music = Cycle(halfMusic, 2).ToArray();

Play(music);


enum N { C, Cs, D, Ds, M, F, Fs, G, Gs, A, As, B }