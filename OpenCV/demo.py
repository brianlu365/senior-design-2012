import cv2

# im = cv2.imread('lena.jpg')
# im_lowres = cv2.pyrDown(im)
# gray = cv2.cvtColor(im_lowres,cv2.COLOR_RGB2GRAY)

# s = cv2.SURF()
# mask = uint8(ones(gray.shape))
# keypoints = s.detect(gray,mask)

# vis = cv2.cvtColor(gray,cv2.COLOR_GRAY2BGR)
# for k in keypoints[::10]:
#   cv2.circle(vis,(int(k.py[0]),int(k.pt[1])),2,(0,255,0),-1)
#   cv2.circle(vis,(int(k.py[0]),int(k.pt[1])),int(k.size),(0,255,0),2)
# cv2.imshow('demo',vis)
# cv2.waitKey()

cap = cv2.VideoCapture(0)

while True:
  ret,im = cap.read()
  blur = cv2.GaussianBlur(im,(0,0),5)
  cv2.imshow('video test',blur)
  key = cv2.waitKey(10)
  if key == 27:
    break
  if key == ord(' '):
    cv2.imwrite('vid_result.jpg',im)
