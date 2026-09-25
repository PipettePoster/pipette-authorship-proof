#:package Magick.NET-Q8-AnyCPU@14.17.1

using ImageMagick;
using ImageMagick.Formats;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

using var httpClient1 = new HttpClient(ProxyHandler());
using var httpClient2 = new HttpClient();

var json1 = await httpClient1.GetStringAsync(
    "https://tantabus.ai/api/v1/json/images/64621");
var json2 = await httpClient2.GetStringAsync(
    "https://desuarchive.org/_/api/chan/post?board=mlp&num=42838122");
var pngBytes = await httpClient2.GetByteArrayAsync(
    "https://tantabuscdn.net/img/view/2025/11/30/64621.png");
var jpeg1Bytes = await httpClient2.GetByteArrayAsync(
    "https://desu-usergeneratedcontent.xyz/mlp/image/1764/45/1764454791848.jpg");

using var png = new MagickImage(pngBytes);
using var jpeg1 = new MagickImage(jpeg1Bytes);
using var jpeg2 = await ToJpeg(png);

await File.WriteAllTextAsync(
    "output.txt",
    json1 + "\n" +
    "\n" +
    json2 + "\n" +
    "\n" +
    Convert.ToHexStringLower(SHA512.HashData(pngBytes)) + "\n" +
    Convert.ToHexStringLower(SHA512.HashData(jpeg1Bytes)) + "\n" +
    Convert.ToHexStringLower(SHA256.HashData(pngBytes)) + "\n" +
    Convert.ToHexStringLower(SHA256.HashData(jpeg1Bytes)) + "\n" +
    Convert.ToBase64String(MD5.HashData(pngBytes)) + "\n" +
    Convert.ToBase64String(MD5.HashData(jpeg1Bytes)) + "\n" +
    "\n" +
    png.Compare(jpeg1, ErrorMetric.PixelDifferenceCount) + "\n" +
    png.Compare(jpeg2, ErrorMetric.PixelDifferenceCount) + "\n" +
    jpeg1.Compare(jpeg2, ErrorMetric.PixelDifferenceCount) + "\n");

SocketsHttpHandler ProxyHandler()
{
    return new SocketsHttpHandler()
    {
        ConnectCallback = async (context, cancellationToken) =>
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            await socket.ConnectAsync(
                new DnsEndPoint(
                    Environment.GetEnvironmentVariable("PROXY_HOST"),
                    int.Parse(Environment.GetEnvironmentVariable("PROXY_PORT"))),
                cancellationToken);

            return new NetworkStream(socket, ownsSocket: true);
        }
    };
}

async Task<MagickImage> ToJpeg(MagickImage image)
{
    using var clone = image.Clone();

    clone.Format = MagickFormat.Jpeg;
    clone.Quality = 98;

    using var memoryStream = new MemoryStream();

    await clone.WriteAsync(
        memoryStream,
        new JpegWriteDefines
        {
            DctMethod = JpegDctMethod.Slow,
            SamplingFactor = JpegSamplingFactor.Ratio444
        });

    return new MagickImage(memoryStream.ToArray());
}
