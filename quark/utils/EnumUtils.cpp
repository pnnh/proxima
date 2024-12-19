#include "EnumUtils.h"

template <typename E>
constexpr typename std::underlying_type<E>::type quark::EnumUtils::ToUnderlying(E e) noexcept
{
    return static_cast<typename std::underlying_type<E>::type>(e);
}
