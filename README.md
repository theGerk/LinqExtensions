# Gerk.LinqExtensions

## [AsyncExtensions.cs](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/AsyncExtensions.cs)
This contains code related to asynchronous operations with Linq.

### SelectAsync
Runs asynchronous function on each element in the collection, projecting to some output. Allows for a concurrency limit.

### ForEachAsync
Runs asynchronous function on each elment in the collection. Allows for a concurrency limit.

### FindMatchAsync
Runs asynchronous predicate on each element in a collection. Returns the first element found that matches, not nessicarily the earlist match in the collection.

Requires .Net Framework 4.7 or great, or .Net Standard 2.0 or greater, or .Net Core 1.0 or greater. (Reason: ValueTuple)

---

## [EfficentLinq](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/EfficentLinq.cs)
This contains code related to Linq that recreates already existing functionality, but more efficently.

### ToArray
Takes a length argument to create an array without having to resize. Saves on having to resize array.

### ToList
Takes a length argument to create a list without having to resize. Saves on having to resize the List.

### FirstIfExists
Identical to FirstOrDefault except it returns a tuple with a return element and a boolean to indicate if there was an element found.

Requires .Net Framework 4.7 or great, or .Net Standard 2.0 or greater, or .Net Core 1.0 or greater. (Reason: ValueTuple)

### LastIfExists
Identical to LastOrDefault except it returns a tuple with a return element and a boolean to indicate if there was an element found.

Requires .Net Framework 4.7 or great, or .Net Standard 2.0 or greater, or .Net Core 1.0 or greater. (Reason: ValueTuple)

### TryFirst
Identical to FirstIfExists except uses an out argument and is compatiable with all supported versions.

### TryLast
Identical to LastIfExists except uses an out argument and is compatiable with all supported versions.

---

## [EnumeratorEnumerable](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/EnumeratorEnumerable.cs)
Internal code. Allows for efficent conversion from IEnumerator to IEnumerable.

---

## [EnumeratorExtensions](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/EnumeratorExtensions.cs)
Extension methods for IEnumerator.

### AsEnumerable
Creates IEnumerable using IEnumerator.
