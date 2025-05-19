#include "hugin/MessagesImpl-swift.h"
#include <iostream>

int fibonacci_cpp(int x) {
  std::cout << "x [cpp]: " << x << std::endl;
  if (x <= 1)
    return 1;
  return HuginLibrary::fibonacciSwift2(x - 1);
}
