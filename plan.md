# Bones Implementation Plan - Requirement 2

## Overview
Implementation plan for enhanced configuration loading and error handling in the Bones application, specifically addressing requirement 2 from requirements.md.

## ✅ COMPLETED IMPLEMENTATION

### Implementation Summary
Successfully implemented all requirement 2 specifications with comprehensive error handling and validation.

### Changes Made

#### 1. **Configuration File Validation** ✅
**File**: `Program.cs`
- Added pre-validation in Main method to check appsettings.json existence
- Uses dynamic executable directory detection with `Assembly.GetExecutingAssembly().Location`
- Displays clear red error message with full file path if missing
- Exits with code 1 on missing configuration

#### 2. **Explicit Configuration Loading** ✅
**File**: `Program.cs` - `CreateHostBuilder` method
- Modified to clear default configuration sources (`config.Sources.Clear()`)
- Loads only from executable directory, no fallback locations
- Ensures predictable behavior regardless of working directory

#### 3. **Enhanced Error Handling** ✅
**File**: `Program.cs`
- Added JSON format validation with early detection
- Specific exception handling for:
  - Missing configuration file (`FileNotFoundException`)
  - Invalid JSON format (`InvalidOperationException`)
  - General configuration errors
- Consistent exit codes: 0 for success, 1 for all errors
- Red error message formatting for all configuration errors

### Implementation Details

**Pre-validation Logic**:
```csharp
// Check for appsettings.json existence before host creation
string executableDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
string configPath = Path.Combine(executableDir, "appsettings.json");

if (!File.Exists(configPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: appsettings.json not found at: {configPath}");
    Console.ResetColor();
    return 1;
}
```

**JSON Validation**:
```csharp
// Validate JSON format before configuration loading
try
{
    var jsonContent = File.ReadAllText(configPath);
    using var document = JsonDocument.Parse(jsonContent);
}
catch (JsonException ex)
{
    throw new InvalidOperationException($"Invalid JSON in {configPath}: {ex.Message}");
}
```

**Explicit Configuration Loading**:
```csharp
.ConfigureAppConfiguration((context, config) =>
{
    // Clear default configuration sources to remove fallbacks
    config.Sources.Clear();
    
    // Add only our specific appsettings.json from executable directory
    config.AddJsonFile(configFilePath, optional: false, reloadOnChange: false);
})
```

### Testing Results ✅

**Scenario 1: Normal Operation**
- ✅ Application runs successfully with valid configuration
- ✅ Exit code: 0

**Scenario 2: Missing Configuration File**
- ✅ Red error message: "Error: appsettings.json not found at: [full path]"
- ✅ Exit code: 1
- ✅ No stack trace or unhandled exceptions

**Scenario 3: Invalid JSON**
- ✅ Red error message: "Error: Invalid JSON in [path]: [detailed error]"
- ✅ Exit code: 1
- ✅ Specific JSON error details included

**Scenario 4: Invalid Arguments**
- ✅ Existing error handling preserved
- ✅ Usage information displayed
- ✅ Exit code: 1

### Deployment Status ✅
- ✅ Application successfully deployed to ~/bin/bones
- ✅ Symlink `bones-exe` working correctly
- ✅ Configuration file properly copied to deployment directory
- ✅ All error scenarios tested and validated

## Requirement 2 Compliance Verification ✅

**Original Requirement 2**: Configuration file loading must load appsettings.json from the same directory as the executable. If the file is missing, the application should display a red error message with the full path to the expected file location and exit with code 1. The application should not attempt to load configuration from any other location (no fallback locations like user profile, etc.).

**Implementation Compliance**:
- ✅ **Same directory loading**: Uses `Assembly.GetExecutingAssembly().Location` to determine executable directory
- ✅ **Missing file handling**: Pre-validation with clear red error message
- ✅ **Full path display**: Shows complete path in error message
- ✅ **Exit code 1**: Proper error exit code for missing configuration
- ✅ **No fallbacks**: `config.Sources.Clear()` removes all default configuration sources
- ✅ **Single location**: Only loads from executable directory

**Additional Enhancements**:
- ✅ **JSON validation**: Detects and reports invalid JSON with specific error details
- ✅ **Consistent error handling**: All configuration errors use same red formatting and exit code
- ✅ **Comprehensive testing**: All scenarios validated including edge cases

## Summary
Requirement 2 has been fully implemented and tested. The Bones application now provides robust configuration loading with clear error messages and predictable behavior. All specifications have been met and the implementation is ready for production use.
    var consoleService = new ConsoleService();
    consoleService.WriteError($"appsettings.json not found at: {configFilePath}");
    return 1;
}
```

### Phase 2: Explicit Configuration Setup

**File**: `Program.cs` - `CreateHostBuilder` method

**Changes Needed**:
1. **Override default configuration** to only load from executable directory
2. **Remove fallback behavior** (no environment variables, no user secrets, etc.)
3. **Ensure single source** for configuration

**Code Changes**:
```csharp
static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            // Clear default configuration sources
            config.Sources.Clear();
            
            // Add only our specific appsettings.json
            var executablePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
            var executableDirectory = Path.GetDirectoryName(executablePath);
            var configFilePath = Path.Combine(executableDirectory, "appsettings.json");
            
            config.AddJsonFile(configFilePath, optional: false, reloadOnChange: false);
        })
        .ConfigureServices((context, services) =>
        {
            // Existing service configuration...
        });
```

### Phase 3: Error Handling Refinement

**File**: `Program.cs` - Main method

**Changes Needed**:
1. **Catch configuration exceptions** specifically
2. **Provide helpful error messages** for config issues
3. **Ensure consistent exit codes**

**Code Changes**:
```csharp
try
{
    var host = CreateHostBuilder(args).Build();
    // ... existing code
}
catch (FileNotFoundException ex) when (ex.FileName?.Contains("appsettings.json") == true)
{
    var consoleService = new ConsoleService();
    consoleService.WriteError($"appsettings.json not found at: {ex.FileName}");
    return 1;
}
catch (JsonException ex)
{
    var consoleService = new ConsoleService();
    consoleService.WriteError($"Invalid JSON in appsettings.json: {ex.Message}");
    return 1;
}
```

## Implementation Steps

### Step 1: Add Configuration File Validation
- [ ] Add pre-validation before host creation
- [ ] Implement dynamic executable directory detection
- [ ] Add specific error message with full path
- [ ] Test with missing appsettings.json

### Step 2: Modify Configuration Loading
- [ ] Override default configuration behavior
- [ ] Remove fallback sources
- [ ] Ensure only executable directory is checked
- [ ] Test configuration loading

### Step 3: Enhance Error Handling
- [ ] Add specific exception handling for config issues
- [ ] Ensure consistent exit codes
- [ ] Test error scenarios

### Step 4: Testing & Validation
- [ ] Test normal operation (config file present)
- [ ] Test missing appsettings.json scenario
- [ ] Test invalid JSON in appsettings.json
- [ ] Verify exit codes
- [ ] Verify error message format and color

## Testing Scenarios

### Positive Tests
1. **Normal Operation**: appsettings.json exists in executable directory
2. **Configuration Loading**: All config values loaded correctly
3. **Service Resolution**: Dependency injection works with loaded config

### Negative Tests
1. **Missing File**: appsettings.json not in executable directory
2. **Invalid JSON**: Malformed JSON in appsettings.json
3. **Empty File**: Empty appsettings.json file

### Validation Criteria
- [ ] Error messages displayed in red
- [ ] Full path included in error messages
- [ ] Exit code 1 for configuration errors
- [ ] No fallback configuration loading
- [ ] Application stops execution on config errors

## Risk Assessment

### Low Risk
- **Breaking existing functionality**: Configuration structure remains the same
- **Deployment issues**: File copying behavior unchanged

### Medium Risk
- **Path resolution**: Different behavior on different operating systems
- **Error handling**: Need to ensure all error scenarios are covered

### Mitigation Strategies
- Use `Path.Combine()` for cross-platform compatibility
- Test on multiple operating systems
- Comprehensive error handling with specific exception types

## Success Criteria

✅ **Functional Requirements**:
- appsettings.json loaded only from executable directory
- Red error message with full path when file missing
- Exit code 1 for missing configuration
- No fallback configuration sources

✅ **Non-Functional Requirements**:
- No breaking changes to existing functionality
- Clear, helpful error messages
- Consistent behavior across platforms
- Fast startup time (no unnecessary file system checks)

## Timeline Estimate

- **Step 1 (Validation)**: 30 minutes
- **Step 2 (Configuration)**: 45 minutes  
- **Step 3 (Error Handling)**: 30 minutes
- **Step 4 (Testing)**: 45 minutes

**Total Estimated Time**: 2.5 hours