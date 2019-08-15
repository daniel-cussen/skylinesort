void skylinesort(uint len, uint * v, uint * t, uint * a, uint * c){
  //len length of
  //v source vec
  //t target vec
  //a auxiliary vec
  //c count vec
  unsigned int e,i,j,p,m=MASK;
  //e element
  //i j indices
  //p position
  //m mask
  for(i=len;i>0;--i){
    //for(i=0;i<len;i++){
    e=v[i];
    c[e]++;
    p=e;
    while(e>a[p]){
      a[p]=e;
      p=(p|(p+1))&m;
    }
  }
  //gather
  p=m;
  for(i=len;i>c[0];i){
    if(a[p]==0)
      p=(((p+1)&p)-1)&m;
    else{
      e=a[p];
      p=a[p];
      while(a[p]){
	a[p]=0;
	p|=p+1;
	p&=m;
      }
      for(j=0;j<c[e];j++)
	t[--i]=e;
      c[e]=0;
      p=e-1;
    }
  }
  while(i>0)
    t[--i]=0;//t[i--]? could be
  c[0]=0;
}
