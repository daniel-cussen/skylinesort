///////////////////////////////////////////////////////////////////
// 			SKYLINESORT ANIMATION
///////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////
//			   initializations
///////////////////////////////////////////////////////////////////

var wW = window.innerWidth;
var wH = window.innerHeight-250;
var canvasHTML = document.getElementById("canvas");
canvasHTML.width = wW;
canvasHTML.height = wH;
var ctx = canvasHTML.getContext("2d");

window.onresize = resize;
var rtime;
var timeout = false;
var delta = 250;

ctx.lineWidth="2";
ctx.strokeStyle="#404040";
ctx.font = "12px Courier";

base = 3+interp(wW,[390,550,950,3300,4000,5000])
rand_min=[2,3,5,7,8,12]
rand_max=[3,5,11,38,92,240]
mask = (1<<base)-1

text_offset = 24;

h_interval = Math.floor((wW-16)/mask)
h_roundoff = (wW-16)-(h_interval*mask+2)
h_padding = 8+Math.floor(h_roundoff/2)

function plot(x0,y0,x1,y1){
    ctx.moveTo(x0,y0)
    ctx.lineTo(x1,y1)
    ctx.stroke()
}

function interp(x,table){
    for(var i=0;i<table.length;i++)
	if(x<=table[i])
            return i
    return table.length
}

v_padding = wH-Math.round(text_offset*1.4)
v_interval = Math.floor((v_padding-text_offset*2)/(1+base))

custom_red="#D10";
building_gray="#333";
building_fill_gray="#AAA";
number_gray="#444";

var aux = [];
var vec = [];
var pos, elt;
var vec_index = 0;
var stack = [];

var animation_speed=120;
var phase = startScatter;
var stage = "unstarted";

if(base==3 || wH < 150){
    announce("Bad dimensions");
} else {
drawPodium(base,h_padding,v_padding);
var source=random_vector(5);
clearAux();
}

////////////////////////////////////////////////////////////////
//window fns
////////////////////////////////////////////////////////////////

function resize() {
    rtime = new Date();
    if (timeout === false) {
        timeout = true;
        setTimeout(resizeend, delta);
    }
};

function resizeend() {
    if (new Date() - rtime < delta) {
        setTimeout(resizeend, delta);
    } else {
        timeout = false;
	location.reload();
    }               
}

////////////////////////////////////////////////////////////////
//graphical fns
////////////////////////////////////////////////////////////////

function announce(str){
    if(stage!="playing")
	document.getElementById("announce").innerText=str;
}

function overwrite(m,n){
    var x = m < 100 ? 16 : 20;
    ctx.clearRect(h_position(m)-8,
		  v_padding+2,
		  x,
		  20);
    printElt(n,h_position(m),v_padding+17);
}

function bold_number(m,n){
    font=ctx.font;
    ctx.font="bold 12px Courier";
    overwrite(m,n);
    ctx.font=font;
}

function highlight(m,n,bool,color){
    if(stage=="paused" && bool)
	bold_number(m,n);
    var style=ctx.strokeStyle;
    ctx.strokeStyle=color;
    drawElt(n,h_position(n),v_padding);
    ctx.strokeStyle=style;
}

function drawPodium(base,x,y){
    var style=ctx.strokeStyle;
    ctx.strokeStyle="#000000";
    ctx.beginPath();
    ctx.moveTo(x-1,y+1);
    ctx.lineTo(x+1+h_interval*mask,y+1);
    ctx.stroke();
    for(var i=0; i<=mask; i++){
        drawElt(i,x+h_interval*i,y);
    }
    ctx.closePath();
    ctx.strokeStyle=style;
}

function printElt(n,x,y){
    var style=ctx.strokeStyle;
    ctx.strokeStyle=number_gray;
    if (base<7) {
	if (n<10 && n>=0) {
            ctx.fillText(n.toString(),x-4,y);
	} else {
            ctx.fillText(n.toString(),x-8,y);
	}	
    } else {
	if (n<10 && n>=0) {
	    ctx.fillText(n.toString(),x-4,y);
	} else if (n<100) {
	    ctx.fillText(n.toString(),x-8,y);
	} else {
	    ctx.fillText(n.toString(),x-10,y);
	}
    }
    ctx.strokeStyle=style;
}

function h_position(n){
    return h_padding + h_interval * n;
}

function drawElt(n, x, y){
    ctx.beginPath();
    ctx.moveTo(x,y);
    ctx.lineTo(x,y-v_interval*find_bit(n));
    ctx.stroke();
    ctx.closePath();
}

function drawRight(n){
    if (n != ior_h(n)){
	ctx.beginPath();
	ctx.moveTo(h_position(n)+1,v_position(n));
	ctx.lineTo(h_position(ior_h(n))-1, v_position(n));
	ctx.stroke();
	ctx.closePath();
    }
}


function deleteRight(n){
    if (n != ior_h(n)){
	ctx.clearRect(h_position(n)+1, 
		      v_position(n)-1, 
		      h_position(ior_h(n))-h_position(n)-2, 
		      2);
    }
}


function drawLeft(n){
    ctx.beginPath();
    ctx.moveTo(h_position(n)-1,v_position(n));
    ctx.lineTo(h_position(ior_hh(n))+1, v_position(n));
    ctx.stroke();
    fill_building(n,ior_hh(n));
    ctx.closePath();
}

function deleteLeft(n){
    ctx.clearRect(h_position(n)-1, 
		  v_position(n)+1, 
		  h_position(ior_hh(n))-h_position(n)+2, 
		  -2);
}

function drawLeftDown(m,n){
    if (m != n){
	ctx.beginPath();
	ctx.moveTo(h_position(m)-1,v_position(m));
	ctx.lineTo(h_position(n)+1,v_position(m));
	ctx.moveTo(h_position(n),v_position(m)-1);
	ctx.lineTo(h_position(n),v_position(n)-1);
	ctx.stroke();
	fill_building(m,n);
	ctx.closePath();
    }
}

function deleteLeftDown(m,n){
    if (m != n){
	ctx.clearRect(h_position(n)-1,
		      v_position(n)-1,
		      2,
		      v_position(m)-v_position(n));
	ctx.clearRect(h_position(n)-1,
		      v_position(m)+1,
		      h_position(m)-h_position(n),
		      -2);
    }
}

function fill_building(m,n){
    var style = ctx.strokeStyle;
    var fill = ctx.fillStyle;
    ctx.beginPath();
    ctx.rect(h_padding+h_interval*n+1,
	     v_padding-(v_interval*find_bit(m))+2,
	     h_interval*(m-n)-1,
	     v_interval*find_bit(m)-1);
    ctx.fillStyle = "#aaa";
    ctx.fill();
    drawPodium(base,h_padding,v_padding);
    ctx.strokeStyle = style;
    ctx.fillStyle = fill;
}

function v_position(n){
    return v_padding - find_bit(n) * v_interval + 1;
}

function find_bit(n){
    var bit = ((n + 1) ^ n) + 1;
    var acc = 0;
    var masks = [0xAAAAAAAA, 0xCCCCCCCC, 
		 0xF0F0F0F0, 0xFF00FF00,
		 0xFFFF0000]
    for(var i=0; i<5; i++){
        if (bit&masks[i]){
            acc += 1<<i;
        }
    }
    return acc;
}           

////////////////////////////////////////////////////////////////
//bitwise fns for skylinesort algorithm
////////////////////////////////////////////////////////////////

function ior_h(n){
    return ((n + 1) | n) & mask;
}

function ior_hh(n){
    return ((n + 1) & n) - 1 & mask;
}

////////////////////////////////////////////////////////////////
//skylinesort algorithm proper
////////////////////////////////////////////////////////////////

function startScatter(){
    announce("Start scatter step");
    ctx.strokeStyle=custom_red;//
    vec=get_vector();
    phase = scatterGetElt;
}

function scatterGetElt(){
    if (vec_index == vec.length){
	phase = startGather;
    } else {
	phase = scatterNext;
    }
    phase();
}

function scatterNext(){
    announce("Scatter next element in the vector");
    elt = vec[vec_index++];
    highlight(elt,elt,1,custom_red);
    pos = elt;
    phase = scatterCompare;
}

function scatterCompare(){
    announce("Compare " + elt + " and " + aux[pos]);
    if (elt > aux[pos]){
	aux[pos]=elt;
	highlight(elt,pos,0,custom_red);
	overwrite(pos,elt);
	phase = scatterDrawRight;
    } else {
	overwrite(elt,elt);
	phase = scatterDeleteElt;
    }
}

function scatterDrawRight(){
    announce("Draw line going right beginning at " + pos + ".");
    drawRight(pos);
    stack.push(pos);
    pos = ior_h(pos);
    phase = scatterCompare;
}

function scatterDeleteElt(){
    announce(elt + " is less than or equal to " + aux[pos] + ". Erase lines going right.");
    while (stack.length > 0)
	deleteRight(stack.pop())
    phase = scatterGetElt;
}

function startGather(){
    announce("End of scatter step. Start gather step.");
    ctx.strokeStyle=building_gray;
    pos = mask;
    vec_index = vec.length-1;
    phase = gatherFindElt;
}

function gatherFindElt(){
    announce("Check if element is zero.");
    pos &= mask;
    if (vec_index==0){
	phase = zeroCorrect;
    } else {
	phase = gatherCheckNext;
    }
}

function gatherCheckNext(){
    announce("Seek next element.");
    if (aux[pos]==0){
	highlight(pos,pos,0,"#000000");
	phase = gatherSkip;
    } else {
	phase = gatherElt;
    }
}

function gatherElt(){
    announce("Element found. Place " +
	     aux[pos] +" in result vector.");
    vec[vec_index--] = aux[pos];
    drawLeftDown(pos, aux[pos]);
    pos = aux[pos]-1;
    phase = gatherFindElt;
}

function gatherSkip(){
    announce("Draw line going left");
    drawLeft(pos);
    pos = ior_hh(pos);
    phase = gatherFindElt;
}

function zeroCorrect(){
    announce("Correct for a possible zero element.");
    if (ior_hh(pos) == mask || aux[pos]!=0){
	drawLeftDown(pos, aux[pos]);
	vec[0] = aux[pos];
	phase = halt;
    } else {
	drawLeft(pos);
	pos = ior_hh(pos);
	phase = zeroCorrect;
    }
}

function halt(){
    terminate_algo();
}

////////////////////////////////////////////////////////////////
//animation fns
////////////////////////////////////////////////////////////////

function rename_play(str){
    var titl={ "Play" : "Run or resume animation",
	       "Pause" : "Pause animation",
	       "Random" : "Restart animation with new random vector"};
    document.getElementById("play_button").innerText=str;
    document.getElementById("play_button").title=titl[str];
}

function rename_step(str){
    var titl={ "Step" : "Play one frame of the animation",
	       "Reset" : "Restart animation with same vector"};
    document.getElementById("step_button").innerText=str;
    document.getElementById("step_button").title=titl[str];
}

function initiate_algo(){
    stage="unstarted";
    phase=startScatter;
    rename_play("Play");
    rename_step("Step");
    announce("Input vector = [" +
	     get_vector() + "]");
}

function terminate_algo(){
    rename_play("Random");
    rename_step("Reset");
    stage="halted";
    announce("Sorting is complete. Result vector = [" +
	     vec + "]");
}

function step_b(e){
    var u = e.keyCode? e.keyCode : e.charCode;
    if (u==13)
	step();//
}

function play_b(e){
    var u = e.keyCode? e.keyCode : e.charCode;
    if (u==13)
	toggle_play();
}

function input_b(e){
    var u = e.keyCode? e.keyCode : e.charCode;
    if (u==13)
	initiate_algo();
    //toggle_play();
}

function step(){
    if(stage=="halted"){
	clear_vector();
	initiate_algo();
    } else {
	rename_play("Play");
	stage="paused";
	phase();
    }
}

function run(){
    if(stage=="playing"){
	phase();
	setTimeout(run,animation_speed);
    }
}

function toggle_play(){
    if (stage=="unstarted"){
	announce("");
	stage="playing";
	rename_play("Pause");
	run();
    } else if (stage=="entering"){
	stage="fixing";
	announce("Autofix");
    } else if (stage=="playing"){
	stage="paused";
	rename_play("Play");
    } else if (stage=="paused"){
	announce("");
	stage="playing";
	rename_play("Pause");
	run();
    } else if (stage=="halted"){
	rename_play("Replay");
	stage="unstarted";
	refresh_vector(Math.floor(Math.random()*rand_max[base-4])+rand_min[base-4]);
	initiate_algo();
    }
}

////////////////////////////////////////////////////////////////
//vector fns
////////////////////////////////////////////////////////////////

function clear_vector(){
    vec_index = 0;
    stack = []
    clearAux();
    ctx.clearRect(0,0,wW,wH);
    drawPodium(base,h_padding,v_padding);
}

function refresh_vector(len){
    clear_vector();
    vec=random_vector(len);
    set_vector(vec);
}

function clearAux(){
    for(var i = 0; i<=mask; i++)
	aux[i]=0;
}

function random_vector(len){
    var vec=[];
    for(var i=0; i < len; i++)
	vec[i]=-1;
    var aux;
    for(var i=0; i < len; i++){
	aux = Math.floor(Math.random()*(1<<base));
	while(vec.indexOf(aux)!=-1)
	    aux = Math.floor(Math.random()*(1<<base));
	vec[i]=aux;
    }
    return vec;
}

function get_vector(){
    return deep_copy(source);
}

function parse_vector(vec){
    return vec.split(/[ ,]+/).map(Number);
}

function set_vector(vec){
    source=vec;
}

function deep_copy(vec){
    var v = [];
    for(var i=0;i<vec.length;i++)
	v[i]=vec[i];
    return v;
}

function remove_duplicates(vec){
    var v=[vec[0]];
    var j=1;
    for(var i=0;i+1<vec.length;i++)
	if(vec[i]!=vec[i+1])
	    v[j++]=vec[i+1];
    return v;
}

function count_duplicates(svec){
    var dupes = 0;
    for(var i=1;i<svec.length;i++)
	if(svec[i-1]==svec[i])
	    dupes+=1;
    return dupes;
}

////////////////////////////////////////////////////////////////
//error detecting and correcting fns
////////////////////////////////////////////////////////////////

function too_many_elts_error(vec){
    if(vec.length>mask){
	return true;
    } else {
	return false;
    }
}

function check_repeats(vec){
    for(var i=0;i<vec.length;i++)
	for(var j=i+1;j<vec.length;j++)
	    if(vec[i]==vec[j])
		return true;
    return false;
}

function elt_size_error(vec){
    for(var i=0;i<vec.length;i++)
	if(vec[i]<0 || vec[i]>mask)
	    return true;
    return false;
}

function decimal_error(vec){
    for(var i=0;i<vec.length;i++)
	if(Math.floor(vec[i])>vec[i])
	    return true;
    return false;
}
