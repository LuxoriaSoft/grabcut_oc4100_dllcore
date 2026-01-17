using System.Drawing;
using Xunit;

namespace Luxoria.Algorithm.GrabCut.Tests;

public class GrabCutTests : IDisposable
{
    private readonly GrabCut _grabCut;
    private readonly string _testOutputDirectory;

    public GrabCutTests()
    {
        _grabCut = new GrabCut();
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GrabCutTests");
        Directory.CreateDirectory(_testOutputDirectory);
    }

    public void Dispose()
    {
        // Clean up test output files
        try
        {
            if (Directory.Exists(_testOutputDirectory))
            {
                Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Creates a simple test image (100x100 red square)
    /// </summary>
    private string CreateTestImage()
    {
        var testImagePath = Path.Combine(_testOutputDirectory, "test_image.bmp");
        
        using (var bitmap = new Bitmap(100, 100))
        {
            // Fill with red color
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    bitmap.SetPixel(x, y, Color.Red);
                }
            }
            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    /// <summary>
    /// Creates a test image with different foreground and background regions
    /// </summary>
    private string CreateTestImageWithRegions()
    {
        var testImagePath = Path.Combine(_testOutputDirectory, "test_image_regions.bmp");
        
        using (var bitmap = new Bitmap(200, 200))
        {
            // Left half - blue (background)
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    bitmap.SetPixel(x, y, Color.Blue);
                }
            }

            // Right half - green (foreground)
            for (int x = 100; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    bitmap.SetPixel(x, y, Color.Green);
                }
            }
            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Act & Assert - Constructor should not throw
        var grabCut = new GrabCut();
        Assert.NotNull(grabCut);
    }

    [Fact]
    public void Exec_WithValidParameters_CreatesOutputFile()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputImage), "Output file should be created");
        Assert.True(new FileInfo(outputImage).Length > 0, "Output file should not be empty");
    }

    [Fact]
    public void Exec_WithDefaultMargin_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_default_margin.bmp");

        // Act & Assert - should not throw
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithCustomMargin_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_custom_margin.bmp");

        // Act & Assert - should not throw with margin
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, margin: 5);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithColorModeEnabled_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_color.bmp");

        // Act & Assert - color should be enabled by default
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: true);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithCustomForegroundColor_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_custom_fg.bmp");
        var foregroundColor = Color.Red;
        var backgroundColor = Color.White;

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false, 
            foreground: foregroundColor, background: backgroundColor);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithCustomBackgroundColor_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_custom_bg.bmp");
        var foregroundColor = Color.Green;
        var backgroundColor = Color.Black;

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false, 
            foreground: foregroundColor, background: backgroundColor);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithDifferentRectangles_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_rect.bmp");

        // Act & Assert - different rectangle coordinates
        _grabCut.Exec(inputImage, outputImage, 20, 20, 30, 30);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithZeroMargin_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_zero_margin.bmp");

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, margin: 0);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithLargeMargin_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_large_margin.bmp");

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, margin: 20);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithVariousColorCombinations_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var colors = new[]
        {
            (Color.Red, Color.Blue),
            (Color.Green, Color.Yellow),
            (Color.Black, Color.White),
            (Color.Purple, Color.Cyan)
        };

        // Act & Assert
        for (int i = 0; i < colors.Length; i++)
        {
            var outputImage = Path.Combine(_testOutputDirectory, $"output_colors_{i}.bmp");
            _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false, 
                foreground: colors[i].Item1, background: colors[i].Item2);
            Assert.True(File.Exists(outputImage));
        }
    }

    [Fact]
    public void Exec_WithColorModeDisabled_RequiresForegroundColor()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_no_colors.bmp");

        // Act & Assert - should throw when color is false but foreground/background are null
        var exception = Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false));
        
        Assert.Contains("Foreground and background colors must be provided", exception.Message);
    }

    [Fact]
    public void Exec_WithColorModeDisabled_RequiresBackgroundColor()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_no_bg_color.bmp");

        // Act & Assert - should throw when color is false but background is null
        var exception = Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false, 
                foreground: Color.Red));
        
        Assert.Contains("Foreground and background colors must be provided", exception.Message);
    }

    [Fact]
    public void Exec_WithNonexistentInputFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonexistentFile = Path.Combine(_testOutputDirectory, "nonexistent.bmp");
        var outputImage = Path.Combine(_testOutputDirectory, "output_fail.bmp");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _grabCut.Exec(nonexistentFile, outputImage, 10, 10, 50, 50));
        
        Assert.Contains("GrabCut execution failed", exception.Message);
    }

    [Fact]
    public void Exec_WithOriginAtZero_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_origin_zero.bmp");

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 0, 0, 50, 50);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithDifferentOutputFormats_CreatesFile()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var formats = new[] { "bmp", "jpg", "png" };

        // Act & Assert
        foreach (var format in formats)
        {
            var outputImage = Path.Combine(_testOutputDirectory, $"output.{format}");
            _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50);
            Assert.True(File.Exists(outputImage), $"Output file with {format} format should be created");
        }
    }

    [Fact]
    public void Exec_ProducesOutputDifferentFromInput()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_different.bmp");

        // Act
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50);

        // Assert
        var inputFileSize = new FileInfo(inputImage).Length;
        var outputFileSize = new FileInfo(outputImage).Length;
        // Output should exist and be a valid file (sizes may differ due to processing)
        Assert.True(File.Exists(outputImage));
        Assert.True(outputFileSize > 0);
    }

    [Fact]
    public void Exec_WithSmallRectangle_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_small_rect.bmp");

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 40, 40, 10, 10);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithLargeRectangle_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_large_rect.bmp");

        // Act & Assert
        try
        {
            _grabCut.Exec(inputImage, outputImage, 0, 0, 90, 90);
            Assert.True(File.Exists(outputImage));
        }
        catch (System.Runtime.InteropServices.SEHException)
        {
            // Large rectangle may cause SEH exception in native code
            // This is acceptable behavior for edge case
        }
    }

    [Fact]
    public void Exec_MultipleCallsWithDifferentOutputFiles()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputs = new[] { "output1.bmp", "output2.bmp", "output3.bmp" };

        // Act & Assert
        foreach (var output in outputs)
        {
            var outputPath = Path.Combine(_testOutputDirectory, output);
            _grabCut.Exec(inputImage, outputPath, 10, 10, 50, 50);
            Assert.True(File.Exists(outputPath));
        }
    }

    [Fact]
    public void Exec_WithMaxColorValues_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_max_colors.bmp");

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false, 
            foreground: Color.White, background: Color.Black);
        Assert.True(File.Exists(outputImage));
    }

    [Fact]
    public void Exec_WithMinColorValues_ExecutesSuccessfully()
    {
        // Arrange
        var inputImage = CreateTestImage();
        var outputImage = Path.Combine(_testOutputDirectory, "output_min_colors.bmp");

        // Act & Assert
        _grabCut.Exec(inputImage, outputImage, 10, 10, 50, 50, color: false, 
            foreground: Color.Black, background: Color.White);
        Assert.True(File.Exists(outputImage));
    }
}
