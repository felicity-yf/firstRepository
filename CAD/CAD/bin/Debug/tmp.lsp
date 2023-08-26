(defun C:GPath()
  (if (gp:getPointInput));
  	(if(Gp:getDialogInput))
  (progn
    (setq PolylineName (gp:drawOutline))
    (princ "\The gp:draOutLine function retured<")
    (princ PolylineName)
    (princ ">")
    (Alert "COngratulations - your program is complete!")
   )
  (princ "\nFunction cancelled.")
  )
(princ "\nIncomplete information to draw a boundary.")
)
(princ)
)
(princ "\nType gpath to draw a garden path.")
(princ)
    