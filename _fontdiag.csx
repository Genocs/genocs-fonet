using SkiaSharp;
using System;
var path = @"src\tests\Genocs.Fonet.Tests\fonts\Nunito-Regular.ttf";
using var tf = SKTypeface.FromFile(path);
Console.WriteLine($"FamilyName: {tf.FamilyName}");
Console.WriteLine($"Style: weight={tf.FontStyle.Weight}, slant={tf.FontStyle.Slant}, italic={tf.IsItalic}, bold={tf.IsBold}");
