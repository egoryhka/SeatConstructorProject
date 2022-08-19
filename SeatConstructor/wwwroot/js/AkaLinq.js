

function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
}

function distinct(arr) {
    return arr.filter(onlyUnique);
}

function select(arr, func) {
    return arr.map(func);
}

function where(arr, func) {
    return arr.filter(func);
}

