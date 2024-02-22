(setq memo-plist '(2 1 1 1))
(setq memoize    (lambda (key value) (cdr (car (setq memo-plist (plist-set memo-plist key value))))))

(setq fib
  (lambda (nth)
    (princ ?\n "Get fib of " nth ?\n)
    
    (let ((memoized (plist-get memo-plist nth)))
      (if memoized
        (progn
          (princ "Use old value " memoized (nl))
          memoized)
        (princ "Calc new value... ") (nl)
        (+ (fib (- nth 1)) (fib (- nth 2))))
      
      )))

(princ "Final fib: " (fib 10) ?\n)
