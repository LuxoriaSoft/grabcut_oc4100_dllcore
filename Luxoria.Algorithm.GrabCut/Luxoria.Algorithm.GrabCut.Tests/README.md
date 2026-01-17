# GrabCut Test Suite

This directory contains a comprehensive test suite for the `Luxoria.Algorithm.GrabCut` library using xUnit.

## Test Projects

### 1. GrabCutTests.cs
**Core functionality tests** covering the main `Exec` method with various scenarios:

- **Basic Execution Tests**
  - Constructor initialization
  - File creation with default parameters
  - Default margin behavior
  - Custom margin values (0, 5, 20)
  - Output in different formats (BMP, JPG, PNG)

- **Color Mode Tests**
  - Color-aware extraction (default mode)
  - Custom foreground and background colors
  - Various color combinations
  - Color validation requirements

- **Rectangle/ROI Tests**
  - Origin at zero
  - Different rectangle positions and sizes
  - Small rectangles
  - Large rectangles
  - Various aspect ratios

- **Error Handling**
  - Missing input file detection
  - Color mode validation (requires colors when disabled)
  - Output file creation verification

### 2. GrabCutParameterValidationTests.cs
**Parameter validation and edge case tests**:

- **Color Parameter Validation**
  - Foreground color requirement when `color=false`
  - Background color requirement when `color=false`
  - Null color handling with `color=true`
  - Individual RGB channel values

- **Rectangle Parameter Edge Cases**
  - Negative coordinates
  - Very small widths/heights (1 pixel)
  - Large rectangle values
  - Various aspect ratios (square, wide, tall)

- **Margin Parameter Variations**
  - Negative margins
  - Zero margin
  - Large margin values

- **Color Conversion**
  - Red channel only
  - Green channel only
  - Blue channel only
  - Same foreground/background colors
  - Min/max color values

### 3. GrabCutIntegrationTests.cs
**Integration tests with complex scenarios**:

- **Complex Image Types**
  - Gradient images
  - Checkerboard patterns
  - Circle overlays
  - Large images (500x500)

- **Real-World Scenarios**
  - Multiple parallel processing
  - Output file overwriting
  - Processing multiple images sequentially
  - Varying ROI on same image
  - Nested output directory creation

- **Batch Operations**
  - Multiple ROI variations on single image
  - Multiple margin values
  - Color mode toggling
  - Multiple color combinations (25+ permutations)

- **Concurrency Tests**
  - Parallel processing with multiple GrabCut instances
  - Thread safety validation

## Running the Tests

### Visual Studio
Open `Luxoria.Algorithm.GrabCut.sln` and use Test Explorer:
```
Test → Test Explorer (Ctrl+E, T)
```

### Command Line (dotnet)
```powershell
cd Luxoria.Algorithm.GrabCut.Tests
dotnet test
```

### Command Line (xUnit)
```powershell
cd Luxoria.Algorithm.GrabCut.Tests
dotnet test --verbosity normal
```

## Test Statistics

- **Total Test Cases**: 60+
- **Test Classes**: 3
- **Coverage Areas**:
  - Basic functionality: 20 tests
  - Parameter validation: 20 tests
  - Integration scenarios: 20+ tests

## Test Naming Conventions

Tests follow the Arrange-Act-Assert (AAA) pattern with descriptive names:
```
[MethodName]_[Condition]_[ExpectedOutcome]
```

Example:
- `Exec_WithCustomMargin_ExecutesSuccessfully`
- `Exec_WithColorModeFalse_RequiresForegroundColor`

## Key Testing Principles

1. **Isolation**: Each test creates its own test images and output directories
2. **Cleanup**: Test data is cleaned up in the `Dispose()` method
3. **Determinism**: Tests use static test images for reproducibility
4. **Clarity**: Test names clearly describe what is being tested
5. **Robustness**: Tests handle both success and failure scenarios

## Test Image Generation

All test images are dynamically generated during test execution:
- **Simple test image**: 100×100 red square (basic tests)
- **Regions image**: 200×200 with blue/green regions (multi-color tests)
- **Gradient image**: 200×200 gradient from black to colored (integration)
- **Checkerboard**: 200×200 alternating black/white (pattern tests)
- **Circle image**: 200×200 with blue circle on white background

## Notes

- Tests use temporary directories (`%TEMP%/GrabCutTests/*`) for output files
- Output files are automatically cleaned up after each test class execution
- The native DLL must be properly embedded in the main library for tests to run
- Tests may take a few seconds depending on system performance
- Some edge case tests may legitimately fail (invalid parameters) and are expected to throw `InvalidOperationException`

## Future Enhancements

- [ ] Performance benchmarking tests
- [ ] Memory usage profiling
- [ ] Large batch processing tests
- [ ] Real image file testing (with sample images)
- [ ] Output quality validation
- [ ] Stress testing with extreme parameters
