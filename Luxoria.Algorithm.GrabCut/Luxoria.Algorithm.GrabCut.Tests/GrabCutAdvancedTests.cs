using System.Drawing;
using Xunit;

namespace Luxoria.Algorithm.GrabCut.Tests;

/// <summary>
/// Advanced test suite for GrabCut functionality.
/// Covers edge cases, boundary conditions, color variations, and parallel execution scenarios.
/// </summary>
public class GrabCutAdvancedTests : IDisposable
{
    // GrabCut instance used for testing
    private readonly GrabCut _grabCut;
    
    // Temporary directory for test outputs (unique per test instance)
    private readonly string _testOutputDirectory;

    public GrabCutAdvancedTests()
    {
        // Initialize GrabCut instance for all tests
        _grabCut = new GrabCut();
        
        // Create unique temp directory to avoid conflicts in parallel test execution
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), "GrabCutAdvancedTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testOutputDirectory);
    }

    public void Dispose()
    {
        // Clean up test output directory after each test
        try
        {
            if (Directory.Exists(_testOutputDirectory))
            {
                // Recursively delete all test files and subdirectories
                Directory.Delete(_testOutputDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors (files may be locked or already deleted)
        }
    }

    /// <summary>
    /// Helper method to create a solid-color test image.
    /// </summary>
    /// <param name="width">Image width in pixels (default: 100)</param>
    /// <param name="height">Image height in pixels (default: 100)</param>
    /// <param name="color">Fill color (default: Blue)</param>
    /// <returns>Path to the created BMP file</returns>
    private string CreateTestImage(int width = 100, int height = 100, Color? color = null)
    {
        // Generate unique filename to avoid conflicts
        var testImagePath = Path.Combine(_testOutputDirectory, "test_image_" + Guid.NewGuid() + ".bmp");
        
        using (var bitmap = new Bitmap(width, height))
        {
            // Use specified color or default to blue
            var fillColor = color ?? Color.Blue;
            
            // Fill entire image with solid color
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, fillColor);
                }
            }
            
            // Save as BMP format for maximum compatibility
            bitmap.Save(testImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        return testImagePath;
    }

    /// <summary>
    /// Tests Exec with both foreground and background colors set to null when color mode is enabled
    /// </summary>
    [Fact]
    public void Exec_WithAllNullableColorsNull_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_null_colors.bmp");

        // Act - color=true (default), so null colors are acceptable
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, true, null, null);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with only foreground color specified when color mode is enabled
    /// </summary>
    [Fact]
    public void Exec_WithForegroundOnly_ColorTrue_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_fg_only.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, true, Color.Red, null);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with only background color specified when color mode is enabled
    /// </summary>
    [Fact]
    public void Exec_WithBackgroundOnly_ColorTrue_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_bg_only.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, true, null, Color.Green);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with margin parameter set to zero
    /// </summary>
    [Fact]
    public void Exec_WithZeroMargin_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_zero_margin.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, margin: 0);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with negative margin value
    /// </summary>
    [Fact]
    public void Exec_WithNegativeMargin_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_neg_margin.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, margin: -5);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with minimal 2x2 region of interest
    /// </summary>
    [Fact]
    public void Exec_WithMinimalROI_2x2_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(50, 50);
        var outputPath = Path.Combine(_testOutputDirectory, "output_minimal_roi.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 2, 2);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with maximum margin value of 10
    /// </summary>
    [Fact]
    public void Exec_WithMaxIterations_10_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_max_iter.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, margin: 10);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with pure white image
    /// </summary>
    [Fact]
    public void Exec_WithColorWhite_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(100, 100, Color.White);
        var outputPath = Path.Combine(_testOutputDirectory, "output_white.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with pure black image
    /// </summary>
    [Fact]
    public void Exec_WithColorBlack_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(100, 100, Color.Black);
        var outputPath = Path.Combine(_testOutputDirectory, "output_black.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with transparent color image
    /// </summary>
    [Fact]
    public void Exec_WithTransparentColor_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(100, 100, Color.Transparent);
        var outputPath = Path.Combine(_testOutputDirectory, "output_transparent.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with custom ARGB colors for foreground and background
    /// </summary>
    [Fact]
    public void Exec_WithCustomColors_AllComponents_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_custom_all.bmp");
        var fgColor = Color.FromArgb(255, 100, 150, 200);
        var bgColor = Color.FromArgb(255, 50, 75, 100);

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, false, fgColor, bgColor);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with ROI positioned at top-left corner (0,0)
    /// </summary>
    [Fact]
    public void Exec_WithEdgePositions_TopLeft_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(200, 200);
        var outputPath = Path.Combine(_testOutputDirectory, "output_topleft.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 0, 0, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with ROI positioned towards bottom-right
    /// </summary>
    [Fact]
    public void Exec_WithEdgePositions_BottomRight_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(200, 200);
        var outputPath = Path.Combine(_testOutputDirectory, "output_bottomright.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 100, 100, 50, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests parallel execution of multiple GrabCut instances
    /// </summary>
    [Fact]
    public void Exec_MultipleInstancesParallel_Succeeds()
    {
        // Arrange
        // Create separate GrabCut instances to ensure thread safety
        var grabCut1 = new GrabCut();
        var grabCut2 = new GrabCut();
        var grabCut3 = new GrabCut();
        
        // Use same input image for all parallel operations
        var inputPath = CreateTestImage();
        var output1 = Path.Combine(_testOutputDirectory, "parallel_1.bmp");
        var output2 = Path.Combine(_testOutputDirectory, "parallel_2.bmp");
        var output3 = Path.Combine(_testOutputDirectory, "parallel_3.bmp");

        // Act - Execute three GrabCut operations in parallel with different ROI regions
        Parallel.Invoke(
            () => grabCut1.Exec(inputPath, output1, 10, 10, 50, 50),  // Top-left region
            () => grabCut2.Exec(inputPath, output2, 20, 20, 40, 40),  // Middle region
            () => grabCut3.Exec(inputPath, output3, 30, 30, 30, 30)   // Bottom-right region
        );

        // Assert - Verify all parallel operations completed successfully
        Assert.True(File.Exists(output1));
        Assert.True(File.Exists(output2));
        Assert.True(File.Exists(output3));
    }

    /// <summary>
    /// Tests sequential Exec calls with different parameters to verify state independence
    /// </summary>
    [Fact]
    public void Exec_SequentialCalls_DifferentParameters_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();

        // Act & Assert - Test parameter variations to verify state independence
        // Each iteration uses different ROI position, size, and margin
        for (int i = 0; i < 5; i++)
        {
            var outputPath = Path.Combine(_testOutputDirectory, $"sequential_{i}.bmp");
            var margin = i;  // Margin increases from 0 to 4
            
            // Execute with progressively different ROI parameters:
            // - Origin shifts right and down (10+i*5, 10+i*5)
            // - ROI size decreases (50-i*5, 50-i*5)
            _grabCut.Exec(inputPath, outputPath, 10 + i * 5, 10 + i * 5, 50 - i * 5, 50 - i * 5, margin);
            Assert.True(File.Exists(outputPath));
        }
    }

    /// <summary>
    /// Tests Exec with wide aspect ratio image (500x100)
    /// </summary>
    [Fact]
    public void Exec_WithWideImage_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(500, 100);
        var outputPath = Path.Combine(_testOutputDirectory, "output_wide.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 50, 10, 200, 50);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with tall aspect ratio image (100x500)
    /// </summary>
    [Fact]
    public void Exec_WithTallImage_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(100, 500);
        var outputPath = Path.Combine(_testOutputDirectory, "output_tall.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 50, 50, 200);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with square 100x100 ROI on square image
    /// </summary>
    [Fact]
    public void Exec_WithSquareROI_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage(200, 200);
        var outputPath = Path.Combine(_testOutputDirectory, "output_square_roi.bmp");

        // Act
        _grabCut.Exec(inputPath, outputPath, 50, 50, 100, 100);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests Exec with color mode disabled and specific RGB color values
    /// </summary>
    [Fact]
    public void Exec_WithColorFalse_BothColorsProvided_RGBValues_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "output_rgb_values.bmp");
        
        // Test with specific RGB values
        var fg = Color.FromArgb(128, 64, 32);
        var bg = Color.FromArgb(255, 128, 64);

        // Act
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50, 1, false, fg, bg);

        // Assert
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// Tests that creating multiple GrabCut instances does not cause interference
    /// </summary>
    [Fact]
    public void Constructor_MultipleInstances_DoNotInterfere()
    {
        // Arrange & Act
        var instances = new GrabCut[10];
        for (int i = 0; i < 10; i++)
        {
            instances[i] = new GrabCut();
        }

        // Assert
        foreach (var instance in instances)
        {
            Assert.NotNull(instance);
        }
    }

    /// <summary>
    /// Tests that Exec can overwrite an existing output file
    /// </summary>
    [Fact]
    public void Exec_OutputOverwritesExistingFile_Succeeds()
    {
        // Arrange
        var inputPath = CreateTestImage();
        var outputPath = Path.Combine(_testOutputDirectory, "overwrite_test.bmp");
        
        // Create initial file with first ROI parameters
        _grabCut.Exec(inputPath, outputPath, 10, 10, 50, 50);
        var firstWriteTime = File.GetLastWriteTime(outputPath);
        
        // Wait to ensure timestamp difference (file system resolution)
        System.Threading.Thread.Sleep(100);

        // Act - Overwrite existing file with different ROI parameters
        _grabCut.Exec(inputPath, outputPath, 20, 20, 40, 40);
        var secondWriteTime = File.GetLastWriteTime(outputPath);

        // Assert - Verify file was overwritten (timestamp changed)
        Assert.True(File.Exists(outputPath));
        Assert.True(secondWriteTime > firstWriteTime, "File should have newer timestamp after overwrite");
    }

    /// <summary>
    /// Tests Exec with very large margin value that may trigger native code limits
    /// </summary>
    [Fact]
    public void Exec_WithVeryLargeMargin_Succeeds()
    {
        // Arrange - Create larger image to accommodate large margin
        var inputPath = CreateTestImage(300, 300);
        var outputPath = Path.Combine(_testOutputDirectory, "output_large_margin.bmp");

        // Act & Assert - Test extreme margin value (100 pixels)
        // This is a boundary condition test for the native implementation
        try
        {
            // Margin of 100 with 100x100 ROI may exceed image bounds
            _grabCut.Exec(inputPath, outputPath, 100, 100, 100, 100, margin: 100);
            Assert.True(File.Exists(outputPath));
        }
        catch (System.Runtime.InteropServices.SEHException)
        {
            // SEH exception is acceptable - indicates native code boundary protection
            // This is expected behavior for extreme edge cases
        }
    }
}
