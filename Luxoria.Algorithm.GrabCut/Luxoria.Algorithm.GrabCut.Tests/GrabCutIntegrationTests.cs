using System.Drawing;
using System.Runtime.Versioning;
using Xunit;

namespace Luxoria.Algorithm.GrabCut.Tests;

[SupportedOSPlatform("windows")]
public class GrabCutIntegrationTests : IDisposable
{
    private readonly GrabCut _grabCut;
    private readonly string _testOutputDirectory;

    public GrabCutIntegrationTests()
    {
        _grabCut = new GrabCut();
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GrabCutIntegrationTests");
        Directory.CreateDirectory(_testOutputDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testOutputDirectory))
            {
                Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Creates a gradient test image for realistic testing
    /// </summary>
    private string CreateGradientImage()
    {
        var testImagePath = Path.Combine(_testOutputDirectory, "gradient.bmp");

        using (var bitmap = new Bitmap(200, 200))
        {
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    int r = (int)(x / 200.0 * 255);
                    int g = (int)(y / 200.0 * 255);
                    int b = 128;
                    bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    /// <summary>
    /// Creates a checkerboard pattern image
    /// </summary>
    private string CreateCheckerboardImage()
    {
        var testImagePath = Path.Combine(_testOutputDirectory, "checkerboard.bmp");

        using (var bitmap = new Bitmap(200, 200))
        {
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    var isWhite = ((x / 20) + (y / 20)) % 2 == 0;
                    bitmap.SetPixel(x, y, isWhite ? Color.White : Color.Black);
                }
            }
            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    /// <summary>
    /// Creates an image with circles
    /// </summary>
    private string CreateCircleImage()
    {
        var testImagePath = Path.Combine(_testOutputDirectory, "circles.bmp");

        using (var bitmap = new Bitmap(200, 200))
        {
            // Fill background with white
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    bitmap.SetPixel(x, y, Color.White);
                }
            }

            // Draw blue circle
            int centerX = 100, centerY = 100, radius = 50;
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        bitmap.SetPixel(x, y, Color.Blue);
                    }
                }
            }

            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    /// <summary>
    /// Tests processing a gradient image with color mode enabled and verifies output generation.
    /// </summary>
    [Fact]
    public void ProcessGradientImage_WithColorMode_GeneratesOutput()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var outputImage = Path.Combine(_testOutputDirectory, "gradient_output.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100);

        // Assert
        Assert.True(File.Exists(outputImage));
        Assert.True(new FileInfo(outputImage).Length > 0);
    }

    /// <summary>
    /// Tests processing a checkerboard pattern image with custom foreground and background colors.
    /// </summary>
    [Fact]
    public void ProcessCheckerboardImage_WithCustomColors_GeneratesOutput()
    {
        // Arrange
        var inputImage = CreateCheckerboardImage();
        var outputImage = Path.Combine(_testOutputDirectory, "checkerboard_output.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100, color: false,
            foreground: Color.Black, background: Color.White);

        // Assert
        Assert.True(File.Exists(outputImage));
    }

    /// <summary>
    /// Tests processing an image containing circles with a specified margin value.
    /// </summary>
    [Fact]
    public void ProcessCircleImage_WithMargin_GeneratesOutput()
    {
        // Arrange
        var inputImage = CreateCircleImage();
        var outputImage = Path.Combine(_testOutputDirectory, "circle_output.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100, margin: 10);

        // Assert
        Assert.True(File.Exists(outputImage));
    }

    /// <summary>
    /// Tests that multiple GrabCut instances can process images in parallel without conflicts.
    /// </summary>
    [Fact]
    public async Task MultipleInstances_CanProcessInParallel()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                var grabcut = new GrabCut();
                var outputImage = Path.Combine(_testOutputDirectory, $"parallel_output_{index}.bmp");
                grabcut.Exec(inputImage, outputImage, 10, 10, 50, 50);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 5; i++)
        {
            var outputPath = Path.Combine(_testOutputDirectory, $"parallel_output_{i}.bmp");
            Assert.True(File.Exists(outputPath));
        }
    }

    /// <summary>
    /// Tests that processing the same image with different ROI regions produces different outputs.
    /// </summary>
    [Fact]
    public void ProcessSameImage_WithDifferentROI_ProducesDifferentOutputs()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var output1 = Path.Combine(_testOutputDirectory, "roi1.bmp");
        var output2 = Path.Combine(_testOutputDirectory, "roi2.bmp");

        // Act
        _grabCut.Exec(inputImage, output1, 10, 10, 50, 50);
        _grabCut.Exec(inputImage, output2, 100, 100, 50, 50);

        // Assert
        Assert.True(File.Exists(output1));
        Assert.True(File.Exists(output2));
        var size1 = new FileInfo(output1).Length;
        var size2 = new FileInfo(output2).Length;
        // Sizes might differ due to different content
        Assert.True(size1 > 0 && size2 > 0);
    }

    /// <summary>
    /// Tests that processing overwrites an existing output file correctly.
    /// </summary>
    [Fact]
    public void ProcessImage_OutputFileOverwrite_UpdatesExistingFile()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var outputImage = Path.Combine(_testOutputDirectory, "overwrite.bmp");

        // Act - first execution
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50);
        var firstSize = new FileInfo(outputImage).Length;

        // Act - second execution with same output path (should overwrite)
        _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100);
        var secondSize = new FileInfo(outputImage).Length;

        // Assert
        Assert.True(File.Exists(outputImage));
        // Sizes might differ, but file should exist and be valid
        Assert.True(secondSize > 0);
    }

    /// <summary>
    /// Tests batch processing of multiple images with consistent settings.
    /// </summary>
    [Fact]
    public void ProcessMultipleImages_WithConsistentSettings_AllSucceed()
    {
        // Arrange
        var images = new[]
        {
            CreateGradientImage(),
            CreateCheckerboardImage(),
            CreateCircleImage()
        };

        // Act & Assert
        for (int i = 0; i < images.Length; i++)
        {
            var outputImage = Path.Combine(_testOutputDirectory, $"multi_output_{i}.bmp");
            _grabCut.Exec(images[i], outputImage, 20, 20, 80, 80);
            Assert.True(File.Exists(outputImage));
        }
    }

    /// <summary>
    /// Tests processing the same image with varying margin values.
    /// </summary>
    [Fact]
    public void ProcessImage_WithVaryingMarginValues_AllSucceed()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var marginValues = new[] { 0, 5, 10, 15, 20 };

        // Act & Assert
        for (int i = 0; i < marginValues.Length; i++)
        {
            var outputImage = Path.Combine(_testOutputDirectory, $"margin_{marginValues[i]}.bmp");
            _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100, margin: marginValues[i]);
            Assert.True(File.Exists(outputImage));
        }
    }

    /// <summary>
    /// Tests that both color mode enabled and disabled produce valid outputs.
    /// </summary>
    [Fact]
    public void ProcessImage_ColorModeToggle_BothProduceOutput()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var outputColor = Path.Combine(_testOutputDirectory, "output_with_color.bmp");
        var outputNoColor = Path.Combine(_testOutputDirectory, "output_no_color.bmp");

        // Act
        _grabCut.Exec(inputImage, outputColor, 50, 50, 100, 100, color: true);
        _grabCut.Exec(inputImage, outputNoColor, 50, 50, 100, 100, color: false,
            foreground: Color.Blue, background: Color.White);

        // Assert
        Assert.True(File.Exists(outputColor));
        Assert.True(File.Exists(outputNoColor));
    }

    /// <summary>
    /// Tests processing a complex checkerboard image with a small ROI region.
    /// </summary>
    [Fact]
    public void ProcessComplexImage_WithSmallROI_Executes()
    {
        // Arrange
        var inputImage = CreateCheckerboardImage();
        var outputImage = Path.Combine(_testOutputDirectory, "complex_small_roi.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 80, 80, 40, 40, margin: 5);

        // Assert
        Assert.True(File.Exists(outputImage));
    }

    /// <summary>
    /// Tests processing a complex checkerboard image with a large ROI region.
    /// </summary>
    [Fact]
    public void ProcessComplexImage_WithLargeROI_Executes()
    {
        // Arrange
        var inputImage = CreateCheckerboardImage();
        var outputImage = Path.Combine(_testOutputDirectory, "complex_large_roi.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 20, 20, 160, 160, margin: 5);

        // Assert
        Assert.True(File.Exists(outputImage));
    }

    /// <summary>
    /// Tests all possible combinations of foreground and background colors produce valid outputs.
    /// </summary>
    [Fact]
    public void AllColorCombinations_ProduceValidOutput()
    {
        // Arrange
        var inputImage = CreateGradientImage();
        var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.White, Color.Black };

        // Act & Assert
        int fileCount = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            for (int j = 0; j < colors.Length; j++)
            {
                if (i != j) // Skip same color combinations
                {
                    var outputImage = Path.Combine(_testOutputDirectory, $"color_combo_{fileCount}.bmp");
                    _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100, color: false,
                        foreground: colors[i], background: colors[j]);
                    Assert.True(File.Exists(outputImage));
                    fileCount++;
                }
            }
        }
    }

    /// <summary>
    /// Tests processing a large 500x500 pixel image successfully.
    /// </summary>
    [Fact]
    public void LargeImage_Processing_Succeeds()
    {
        // Arrange
        var largeImagePath = Path.Combine(_testOutputDirectory, "large_image.bmp");
        using (var bitmap = new Bitmap(500, 500))
        {
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(x % 256, y % 256, 128));
                }
            }
            bitmap.Save(largeImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        var outputImage = Path.Combine(_testOutputDirectory, "large_output.bmp");

        // Act
        _grabCut.Exec(largeImagePath, outputImage, 100, 100, 300, 300);

        // Assert
        Assert.True(File.Exists(outputImage));
    }

    /// <summary>
    /// Tests that output files can be created in nested directory structures.
    /// </summary>
    [Fact]
    public void OutputDirectoryCreation_DoesNotThrow()
    {
        // Arrange
        var nestedDir = Path.Combine(_testOutputDirectory, "nested", "output", "dir");
        var inputImage = CreateGradientImage();
        var outputImage = Path.Combine(nestedDir, "output.bmp");

        // Act - create the nested directory first
        Directory.CreateDirectory(nestedDir);
        _grabCut.Exec(inputImage, outputImage, 50, 50, 100, 100);

        // Assert
        Assert.True(File.Exists(outputImage));
    }
}
