void skylinesort(uint len, uint * vec, uint * tgt, uint * aux, uint * cnt){
  //vec source vec
  //tgt target vec
  //aux auxiliary vec
  //cnt count vec
  unsigned int elt,i,j,pos,mask=MASK;
  //elt element
  //i j indices
  //pos position
  for(i=len;i>0;i){
    elt=vec[--i];
    cnt[elt]++;
    pos=elt;
    while(elt>aux[pos]){
      aux[pos]=elt;
      pos=((pos+1)|pos)&mask;
    }
  }
  //gather
  pos=mask;
  for(i=len;i>cnt[0];i){
    if(aux[pos]==0)
      pos=(((pos+1)&pos)-1)&mask;
    else{
      elt=aux[pos];
      pos=aux[pos];
      while(aux[pos]){
	aux[pos]=0;
	pos|=pos+1;
	pos&=mask;
      }
      for(j=0;j<cnt[elt];j++)
	tgt[--i]=elt;
      cnt[elt]=0;
      pos=elt-1;
    }
  }
  while(i>0)
    tgt[--i]=0;
  cnt[0]=0;
}
