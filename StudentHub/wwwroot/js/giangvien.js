document.getElementById('sidebarToggle')?.addEventListener('click',()=>document.getElementById('sidebar')?.classList.toggle('show'));
document.querySelectorAll('[data-copy]').forEach(button=>button.addEventListener('click',async()=>{await navigator.clipboard.writeText(button.dataset.copy);const old=button.innerHTML;button.innerHTML='<i class="bi bi-check"></i> Đã sao chép';setTimeout(()=>button.innerHTML=old,1500)}));
function renderAttendanceQr(url){const target=document.getElementById('qrcode');if(!target||!url)return;new QRCode(target,{text:url,width:200,height:200,colorDark:'#0f172a',colorLight:'#ffffff',correctLevel:QRCode.CorrectLevel.H})}
function startAttendanceCountdown(startMs,endMs,serverNowMs,isOpen){
 const value=document.getElementById('sessionCountdown'),label=document.getElementById('countdownLabel'),panel=document.getElementById('countdownPanel');if(!value)return;
 const clientStarted=Date.now();let timer;
 const format=ms=>{const total=Math.max(0,Math.floor(ms/1000)),h=Math.floor(total/3600),m=Math.floor(total%3600/60),s=total%60;return [h,m,s].map(x=>String(x).padStart(2,'0')).join(':')};
 const endSession=()=>{label.textContent='Phiên điểm danh';value.textContent='ĐÃ KẾT THÚC';panel?.classList.add('expired');document.getElementById('qrcode')?.classList.add('qr-expired');const status=document.getElementById('sessionStatus');if(status){status.className='badge text-bg-secondary mb-3';status.textContent='Đã hết thời gian'}document.getElementById('copyCheckIn')?.setAttribute('disabled','disabled');document.getElementById('sessionToggle')?.setAttribute('disabled','disabled');if(timer)clearInterval(timer)};
 const tick=()=>{const now=serverNowMs+(Date.now()-clientStarted);if(now<startMs){label.textContent='Bắt đầu sau';value.textContent=format(startMs-now);panel?.classList.remove('active');return}if(now>=endMs){endSession();return}label.textContent=isOpen?'Thời gian còn lại':'Phiên đang đóng · Thời gian còn lại';value.textContent=format(endMs-now);panel?.classList.add('active')};
 tick();timer=setInterval(tick,1000)
}
