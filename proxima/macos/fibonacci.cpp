
#include "MTProxima-swift.h"

#include <iostream>
#include "fibonacci.h"

int fibo::calcFibonacci() {
  auto abc = MTProxima::fibonacciSwift(5);
  std::cout << "abc: " << abc << std::endl;
  return 0;
}
