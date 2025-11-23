
@LOWER = global i32 -10
@UPPER = global i32 10

define i32 @main() {
entry:
  ; Simulate setting config constants
  store i32 -10, i32* @LOWER
  store i32 10, i32* @UPPER

  %a = alloca i32
  %b = alloca i32
  %c = alloca i32

  store i32 5, i32* %a          ; a = 5
  store i32 3, i32* %b          ; b = 3

  %val_a = load i32, i32* %a
  %val_b = load i32, i32* %b
  %sum = add i32 %val_a, %val_b  ; sum = a + b => 8 (OK)

  store i32 %sum, i32* %c

  %loaded_c = load i32, i32* %c
  %res = mul i32 %loaded_c, 2    ; 8 * 2 = 16 (out of bounds)

  ret i32 %res
}
