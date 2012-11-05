@ECHO OFF

REM ========================================================================
REM Clean Up Batch File
REM ------------------------------------------------------------------------
REM This batch file removes all temporary build files that should not be
REM distributed with the application.  The result will be a smaller ZIP file.
REM ========================================================================


RMDIR /Q /S "Samples\Quote of the Day\Quote\bin"
RMDIR /Q /S "Samples\Quote of the Day\Quote\obj"
RMDIR /Q /S "Samples\Logon Prompt\Logon Prompt\bin"
RMDIR /Q /S "Samples\Logon Prompt\Logon Prompt\obj"
RMDIR /Q /S "Samples\Aurora City\Aurora\bin"
RMDIR /Q /S "Samples\Aurora City\Aurora\obj"
RMDIR /Q /S "Samples\Tutorial\Step1\bin"
RMDIR /Q /S "Samples\Tutorial\Step1\obj"
RMDIR /Q /S "Samples\Tutorial\Step2\bin"
RMDIR /Q /S "Samples\Tutorial\Step2\obj"
RMDIR /Q /S "Samples\Tutorial\Step3\bin"
RMDIR /Q /S "Samples\Tutorial\Step3\obj"
RMDIR /Q /S "Samples\Tutorial\Step4\bin"
RMDIR /Q /S "Samples\Tutorial\Step4\obj"
RMDIR /Q /S "Samples\Tutorial\Step5\bin"
RMDIR /Q /S "Samples\Tutorial\Step5\obj"
RMDIR /Q /S "Samples\Tutorial\Step6\bin"
RMDIR /Q /S "Samples\Tutorial\Step6\obj"
RMDIR /Q /S "Solution\Thought.Terminals\bin"
RMDIR /Q /S "Solution\Thought.Terminals\obj"
RMDIR /Q /S "Unit Tests\Thought.Terminals.Tests\bin"
RMDIR /Q /S "Unit Tests\Thought.Terminals.Tests\obj"
DEL "Unit Tests\TestResult.xml"
PAUSE
