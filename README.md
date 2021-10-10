# Gerk.LinqExtensions

## [AsyncExtensions.cs](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/AsyncExtensions.cs)
This contains code related to asynchronous operations with Linq.

### SelectAsync
Runs asynchronous function on each element in the collection, projecting to some output. Allows for a concurrency limit.

### ForEachAsync
Runs asynchronous function on each elment in the collection. Allows for a concurrency limit.

---

## [EfficentLinq](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/EfficentLinq.cs)
This contains code related to Linq that recreates already existing functionality, but more efficently.

### ToArray
Takes a length argument to create an array without having to resize. Saves on having to resize array.

### ToList
Takes a length argument to create a list without having to resize. Saves on having to resize the List.

---

## [EnumeratorEnumerable](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/EnumeratorEnumerable.cs)
Internal code. Allows for efficent conversion from IEnumerator to IEnumerable.

---

## [EnumeratorExtensions](https://github.com/theGerk/LinqExtensions/blob/master/Gerk.LinqExtensions/EnumeratorExtensions.cs)
Extension methods for IEnumerator.

### AsEnumerable
Creates IEnumerable using IEnumerator.
