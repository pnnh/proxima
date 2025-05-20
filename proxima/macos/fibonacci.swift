@_expose(Cxx)
public func fibonacciSwift(_ x: CInt) -> CInt {
    print("x [swift]: \(x)")
    if x <= 1 {
        return 1
    }
    return 333
}
