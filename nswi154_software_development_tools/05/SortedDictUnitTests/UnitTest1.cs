namespace SortedDictUnitTests;
using System;
using System.Collections.Generic;
using System.Linq;

public class SortedDictionaryUnitTests
{
    /// <summary>
    /// Adding new mapping with the key different from all present mappings
    /// </summary>
    [Fact]
    public void AddingNewKeyValPair()
    {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        //Act
        dict.Add("two", 2);
        //Assert
        Assert.Equal(2, dict.Count);
        Assert.True(dict.ContainsKey("two"));
        Assert.Equal(2, dict["two"]);
    }

    /// <summary>
    /// Adding new mapping with a key equal to some already present mapping
    /// </summary>
    [Fact]
    public void AddingNewMappingWithOldKey() {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        //Act and Assert
        Assert.Throws<ArgumentException>(() => dict.Add("one", 2));
        Assert.Single(dict);
        Assert.Equal(1, dict["one"]);
    }

    /// <summary>
    /// Removing some element (mapping) already present in the map
    /// </summary>
    [Fact]
    public void RemovingElementWithValidKey() {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);
        //Act
        dict.Remove("one");
        //Assert
        Assert.Equal(2, dict.Count);
        Assert.False(dict.ContainsKey("one"));
    }

    /// <summary>
    /// Removing all elements from the map and checking for emptiness
    /// </summary>
    [Fact]
    public void RemovingAllElements() {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);
        //Act
        dict.Clear();
        //Assert
        Assert.Empty(dict);
    }

    /// <summary>
    /// Getting the value associated with a specific key
    /// </summary>
    [Fact]
    public void GettingValueOfValidKey() {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);
        //Act
        int value = dict["one"];
        //Assert
        Assert.Equal(1, value);
    }

    /// <summary>
    /// Attempt to get a value for a key not present in the map
    /// </summary>
    [Fact]
    public void GettingValueOfInvalidKey() {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);
        //Act and Assert
        Assert.Throws<KeyNotFoundException>(() => {int value = dict["four"];});
        Assert.Equal(3, dict.Count);
    }

    class WrongKeyValuePairException : Exception {
        public WrongKeyValuePairException() { }
        public WrongKeyValuePairException(string message) : base(message) { }
        public WrongKeyValuePairException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Assert that the map has a key-value mapping which it does not really have
    /// </summary>
    [Fact]
    public void TryingToGetInvalidValueOfValidKey() {
        //Arrange
        SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        dict.Add("one", 1);
        dict.Add("two", 2);
        dict.Add("three", 3);
        //Act
        var value = dict["three"];
        //Assert
        //Assert.Throws<WrongKeyValuePairException>(() => {if (value != 4) throw new WrongKeyValuePairException("no such key-value pair");});
        if (value != 4) throw new WrongKeyValuePairException("no such key-value pair");
    }
}