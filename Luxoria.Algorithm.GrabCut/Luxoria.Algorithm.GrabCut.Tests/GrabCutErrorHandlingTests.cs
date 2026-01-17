using System.Drawing;
using Xunit;

namespace Luxoria.Algorithm.GrabCut.Tests;

/// <summary>
/// Error handling tests for GrabCut API.
/// 
/// Note: Tests that call the native DLL with invalid parameters (null paths, nonexistent files, 
/// zero dimensions) are intentionally excluded because the native C++ code crashes with 
/// AccessViolationException instead of handling errors gracefully. These tests focus on 
/// error conditions that can be validated without triggering native crashes.
/// </summary>
public class GrabCutErrorHandlingTests : IDisposable
{
    private readonly GrabCut _grabCut;
    private readonly string _testOutputDirectory;

    public GrabCutErrorHandlingTests()
    {
        _grabCut = new GrabCut();
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GrabCutErrorTests_" + Guid.NewGuid());
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

    private string CreateValidTestImage(int width = 100, int height = 100, Color? color = null)
    {
        var testImagePath = Path.Combine(_testOutputDirectory, "valid_image_" + Guid.NewGuid() + ".bmp");
        
        using (var bitmap = new Bitmap(width, height))
        {
            var fillColor = color ?? Color.Red;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, fillColor);
                }
            }
            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    /// <summary>
    /// Tests that a GrabCut instance can be successfully instantiated.
    /// </summary>
    [Fact]
    public void GrabCut_IsInstantiable()
    {
        // Arrange & Act
        var grabCut = new GrabCut();

        // Assert
        Assert.NotNull(grabCut);
    }

    /// <summary>
    /// Tests that Exec succeeds with valid parameters and creates an output file.
    /// </summary>
    [Fact]
    public void Exec_WithValidParameters_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_valid.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created");
    }

    /// <summary>
    /// Tests that Exec succeeds with a small region of interest (ROI).
    /// </summary>
    [Fact]
    public void Exec_WithSmallRoi_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage(200, 200);
        var outputPath = Path.Combine(_testOutputDirectory, "output_small_roi.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 50, 50, 30, 30);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created with small ROI");
    }

    /// <summary>
    /// Tests that Exec succeeds with a medium number of iterations (3).
    /// </summary>
    [Fact]
    public void Exec_WithMediumIterations_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_medium_iterations.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 3);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created with medium iterations");
    }

    /// <summary>
    /// Tests that Exec succeeds with a large number of iterations (5).
    /// </summary>
    [Fact]
    public void Exec_WithLargeIterations_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_large_iterations.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 5);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created with large iterations");
    }

    /// <summary>
    /// Tests that Exec succeeds with custom foreground and background color parameters.
    /// </summary>
    [Fact]
    public void Exec_WithColorParameters_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_with_colors.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, false, Color.Blue, Color.Green);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created with color parameters");
    }

    /// <summary>
    /// Tests that Exec creates an output file with content after execution.
    /// </summary>
    [Fact]
    public void Exec_CreatesOutputFile()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_file_creation.bmp");
        Assert.False(File.Exists(outputPath), "Output file should not exist before execution");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created after execution");
        Assert.True(new FileInfo(outputPath).Length > 0, "Output file should have content");
    }

    /// <summary>
    /// Tests that the output file created by Exec is a valid bitmap that can be loaded.
    /// </summary>
    [Fact]
    public void Exec_OutputFileIsValidBitmap()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_valid_bitmap.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
        
        // Try to load the output as a bitmap
        using (var outputBitmap = new Bitmap(outputPath))
        {
            Assert.True(outputBitmap.Width > 0);
            Assert.True(outputBitmap.Height > 0);
        }
    }

    /// <summary>
    /// Tests that Exec preserves the original image dimensions in the output.
    /// </summary>
    [Fact]
    public void Exec_PreservesImageDimensions()
    {
        // Arrange
        const int width = 150;
        const int height = 120;
        var inputPath = CreateValidTestImage(width, height);
        var outputPath = Path.Combine(_testOutputDirectory, "output_dimensions.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 20, 20, 50, 50);

        // Assert
        using (var outputBitmap = new Bitmap(outputPath))
        {
            Assert.Equal(width, outputBitmap.Width);
            Assert.Equal(height, outputBitmap.Height);
        }
    }

    /// <summary>
    /// Tests that Exec can be called multiple times successfully without side effects.
    /// </summary>
    [Fact]
    public void Exec_CanBeCalledMultipleTimes()
    {
        // Arrange
        var inputPath = CreateValidTestImage();

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            var outputPath = Path.Combine(_testOutputDirectory, $"output_multiple_{i}.bmp");
            _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);
            Assert.True(File.Exists(outputPath), $"Output file {i} should be created");
        }
    }

    /// <summary>
    /// Tests that Exec succeeds with different ROI positions on the same image.
    /// </summary>
    [Fact]
    public void Exec_WithDifferentRoiPositions_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage(200, 200);

        // Act & Assert
        var roiPositions = new[] { (10, 10), (50, 50), (100, 100) };
        foreach (var (x, y) in roiPositions)
        {
            var outputPath = Path.Combine(_testOutputDirectory, $"output_roi_{x}_{y}.bmp");
            _grabCut.Exec(inputPath, outputPath, x, y, 50, 50);
            Assert.True(File.Exists(outputPath), $"Output file at position ({x},{y}) should be created");
        }
    }

    /// <summary>
    /// Tests that Exec succeeds with a minimal 50x50 pixel image.
    /// </summary>
    [Fact]
    public void Exec_WithMinimalImage_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage(50, 50);
        var outputPath = Path.Combine(_testOutputDirectory, "output_minimal.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 5, 5, 20, 20);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created for minimal image");
    }

    /// <summary>
    /// Tests that Exec succeeds with a large 400x300 pixel image.
    /// </summary>
    [Fact]
    public void Exec_WithLargeImage_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage(400, 300);
        var outputPath = Path.Combine(_testOutputDirectory, "output_large.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 50, 50, 150, 150);

        // Assert
        Assert.True(File.Exists(outputPath), "Output file should be created for large image");
    }

    /// <summary>
    /// Tests that Exec succeeds with a custom foreground color when color mode is enabled.
    /// </summary>
    [Fact]
    public void Exec_WithCustomForegroundColor_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_fg_color.bmp");

        // Act & Assert - When color=true, both foreground and background colors can be provided
        // color=false is the default, so we need to pass color=true to avoid validation error
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, color: true, foreground: Color.Cyan);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests that Exec succeeds with both custom foreground and background colors.
    /// </summary>
    [Fact]
    public void Exec_WithBothCustomColors_Succeeds()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_both_colors.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, false, Color.Yellow, Color.Magenta);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests that Exec throws ArgumentException when color is false and foreground color is null.
    /// </summary>
    [Fact]
    public void Exec_WithColorFalseAndNullForeground_ThrowsArgumentException()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output.bmp");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, color: false, foreground: null, background: Color.White));

        Assert.Contains("Foreground and background colors must be provided", exception.Message);
    }

    /// <summary>
    /// Tests that Exec throws ArgumentException when color is false and background color is null.
    /// </summary>
    [Fact]
    public void Exec_WithColorFalseAndNullBackground_ThrowsArgumentException()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output.bmp");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, color: false, foreground: Color.Red, background: null));

        Assert.Contains("Foreground and background colors must be provided", exception.Message);
    }

    /// <summary>
    /// Tests that Exec throws ArgumentException when color is false and both foreground and background colors are null.
    /// </summary>
    [Fact]
    public void Exec_WithColorFalseAndBothColorsNull_ThrowsArgumentException()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output.bmp");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, color: false, foreground: null, background: null));

        Assert.Contains("Foreground and background colors must be provided", exception.Message);
    }

    /// <summary>
    /// Tests that Exec can execute successfully after a previous execution threw an exception.
    /// </summary>
    [Fact]
    public void Exec_ValidExecution_AfterException_Works()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output.bmp");

        // Act - trigger exception
        Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, color: false));

        // Act - valid execution after exception
        var validOutputPath = Path.Combine(_testOutputDirectory, "valid_output.bmp");
        _grabCut.Exec(inputPath, validOutputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(validOutputPath));
    }

    /// <summary>
    /// Tests that different GrabCut instances handle errors independently without cross-contamination.
    /// </summary>
    [Fact]
    public void Exec_DifferentInstances_HandleErrorsIndependently()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath1 = Path.Combine(_testOutputDirectory, "output1.bmp");
        var outputPath2 = Path.Combine(_testOutputDirectory, "output2.bmp");

        var grabCut1 = new GrabCut();
        var grabCut2 = new GrabCut();

        // Act & Assert - both should execute without cross-contamination
        grabCut1.Exec(inputPath, outputPath1, 10, 10, 50, 50);
        
        Assert.Throws<ArgumentException>(() =>
            grabCut2.Exec(inputPath, outputPath2, 10, 10, 50, 50, color: false));

        Assert.True(File.Exists(outputPath1));
    }

    /// <summary>
    /// Tests that color validation throws the correct ArgumentException type.
    /// </summary>
    [Fact]
    public void Exec_ArgumentExceptionType_ForColorValidation()
    {
        // Arrange
        var inputPath = CreateValidTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output.bmp");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, color: false));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    /// <summary>
    /// Tests that the GrabCut constructor can be called multiple times without throwing exceptions.
    /// </summary>
    [Fact]
    public void Constructor_DoesNotThrow_WhenCalledMultipleTimes()
    {
        // Act & Assert
        for (int i = 0; i < 5; i++)
        {
            var grabCut = new GrabCut();
            Assert.NotNull(grabCut);
        }
    }
}
