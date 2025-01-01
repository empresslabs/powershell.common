// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using Empress.Labs.PowerShell.Common.ExtendedDataAnnotations;

namespace Empress.Labs.PowerShell.Common.UnitTesting.ExtendedDataAnnotations;

[TestFixture]
[TestOf(typeof(NotNullOrEmptyAttribute))]
public class NotNullOrEmptyAttributeTest {
  [Test]
  public void AttributeUsage_IsNotValid_WhenValueIsNull() {
    // Arrange
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid(null);

    // Assert
    Assert.That(isValid, Is.False);
  }

  [Test]
  public void AttributeUsage_IsNotValid_WhenValueIsEmptyString() {
    // Arrange
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid(string.Empty);

    // Assert
    Assert.That(isValid, Is.False);
  }

  [Test]
  public void AttributeUsage_IsValid_WhenValueIsString() {
    // Arrange
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid("test");

    // Assert
    Assert.That(isValid, Is.True);
  }

  [Test]
  public void AttributeUsage_IsNotValid_WhenValueIsEmptyArray() {
    // Arrange
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid(Array.Empty<int>());

    // Assert
    Assert.That(isValid, Is.False);
  }

  [Test]
  public void AttributeUsage_IsValid_WhenValueIsArray() {
    // Arrange
    var array = new[] { 1 };
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid(array);

    // Assert
    Assert.That(isValid, Is.True);
  }

  [Test]
  public void AttributeUsage_IsNotValid_WhenValueIsEmptyCollection() {
    // Arrange
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid(new List<int>());

    // Assert
    Assert.That(isValid, Is.False);
  }

  [Test]
  public void AttributeUsage_IsValid_WhenValueIsCollection() {
    // Arrange
    var collection = new List<int> { 1 };
    var attribute = new NotNullOrEmptyAttribute();

    // Act
    var isValid = attribute.IsValid(collection);

    // Assert
    Assert.That(isValid, Is.True);
  }
}
