using System.ComponentModel;
using SimpleLauncher.Services.CheckApplicationControlPolicy;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="CheckApplicationControlPolicy"/> static utility methods
/// covering application control policy detection, elevation requirements, and UAC cancellation.
/// </summary>
public class CheckApplicationControlPolicyTests
{
    // IsApplicationControlPolicyBlocked tests

    [Fact]
    public void IsApplicationControlPolicyBlockedWithInvalidOperationExceptionReturnsFalse()
    {
        var ex = new InvalidOperationException("test");
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithWin32AccessDeniedAndEnglishMessageReturnsTrue()
    {
        var ex = new Win32Exception(5, "Application Control policy blocked the operation");
        Assert.True(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithWin32AccessDeniedAndSpanishMessageReturnsTrue()
    {
        var ex = new Win32Exception(5, "Control de aplicaciones bloqueó la ejecución");
        Assert.True(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithWin32AccessDeniedButUnrelatedMessageReturnsFalse()
    {
        var ex = new Win32Exception(5, "Access is denied");
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithWin32NonAccessDeniedCodeReturnsFalse()
    {
        var ex = new Win32Exception(2, "Application Control policy blocked");
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithWin32AccessDeniedCaseInsensitiveReturnsTrue()
    {
        var ex = new Win32Exception(5, "APPLICATION CONTROL POLICY BLOCKED");
        Assert.True(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithWin32AccessDeniedMixedCaseSpanishReturnsTrue()
    {
        var ex = new Win32Exception(5, "CONTROL DE APLICACIONES BLOQUEÓ");
        Assert.True(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithNullMessageReturnsFalse()
    {
        var ex = new Win32Exception(5);
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    // IsElevationRequired tests

    [Fact]
    public void IsElevationRequiredWithWin32Exception740ReturnsTrue()
    {
        var ex = new Win32Exception(740);
        Assert.True(CheckApplicationControlPolicy.IsElevationRequired(ex));
    }

    [Fact]
    public void IsElevationRequiredWithWin32ExceptionOtherCodeReturnsFalse()
    {
        var ex = new Win32Exception(5);
        Assert.False(CheckApplicationControlPolicy.IsElevationRequired(ex));
    }

    [Fact]
    public void IsElevationRequiredWithInvalidOperationExceptionReturnsFalse()
    {
        var ex = new InvalidOperationException();
        Assert.False(CheckApplicationControlPolicy.IsElevationRequired(ex));
    }

    [Fact]
    public void IsElevationRequiredWithWin32Exception740AndMessageReturnsTrue()
    {
        var ex = new Win32Exception(740, "The requested operation requires elevation");
        Assert.True(CheckApplicationControlPolicy.IsElevationRequired(ex));
    }

    // IsOperationCanceledByUser tests

    [Fact]
    public void IsOperationCanceledByUserWithWin32Exception1223ReturnsTrue()
    {
        var ex = new Win32Exception(1223);
        Assert.True(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    [Fact]
    public void IsOperationCanceledByUserWithWin32ExceptionOtherCodeReturnsFalse()
    {
        var ex = new Win32Exception(5);
        Assert.False(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    [Fact]
    public void IsOperationCanceledByUserWithInvalidOperationExceptionReturnsFalse()
    {
        var ex = new InvalidOperationException();
        Assert.False(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    [Fact]
    public void IsOperationCanceledByUserWithWin32Exception1223AndMessageReturnsTrue()
    {
        var ex = new Win32Exception(1223, "The operation was canceled by the user");
        Assert.True(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    // Edge case tests

    [Fact]
    public void IsApplicationControlPolicyBlockedWithAggregateExceptionReturnsFalse()
    {
        var ex = new AggregateException("test");
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsElevationRequiredWithAggregateExceptionReturnsFalse()
    {
        var ex = new AggregateException("test");
        Assert.False(CheckApplicationControlPolicy.IsElevationRequired(ex));
    }

    [Fact]
    public void IsOperationCanceledByUserWithAggregateExceptionReturnsFalse()
    {
        var ex = new AggregateException("test");
        Assert.False(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    [Fact]
    public void AllMethodsReturnFalseForFileNotFoundException()
    {
        var ex = new FileNotFoundException("file not found");
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
        Assert.False(CheckApplicationControlPolicy.IsElevationRequired(ex));
        Assert.False(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    [Fact]
    public void AllMethodsReturnFalseForDirectoryNotFoundException()
    {
        var ex = new DirectoryNotFoundException("dir not found");
        Assert.False(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
        Assert.False(CheckApplicationControlPolicy.IsElevationRequired(ex));
        Assert.False(CheckApplicationControlPolicy.IsOperationCanceledByUser(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithPartialEnglishMatchReturnsTrue()
    {
        var ex = new Win32Exception(5, "The Application Control policy blocked this app from running");
        Assert.True(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }

    [Fact]
    public void IsApplicationControlPolicyBlockedWithPartialSpanishMatchReturnsTrue()
    {
        var ex = new Win32Exception(5, "El Control de aplicaciones bloqueó esta aplicación");
        Assert.True(CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex));
    }
}
