document.getElementById('sidebarToggle')?.addEventListener('click',()=>document.getElementById('sidebar')?.classList.toggle('show'));
document.querySelectorAll('.delete-form').forEach(form=>form.addEventListener('submit',event=>{if(!confirm('Bạn chắc chắn muốn xóa dữ liệu này?'))event.preventDefault()}));
setTimeout(()=>document.querySelectorAll('.alert').forEach(el=>bootstrap.Alert.getOrCreateInstance(el).close()),5000);
function renderAdminCharts(khoaLabels,khoaValues,hocKyLabels,hocKyValues){
 new Chart(document.getElementById('khoaChart'),{type:'doughnut',data:{labels:khoaLabels,datasets:[{data:khoaValues,backgroundColor:['#2563eb','#10b981','#f59e0b','#ef4444','#8b5cf6','#06b6d4']}]},options:{responsive:true,maintainAspectRatio:false}});
 new Chart(document.getElementById('hocKyChart'),{type:'bar',data:{labels:hocKyLabels,datasets:[{label:'Số lớp',data:hocKyValues,backgroundColor:'#2563eb',borderRadius:6}]},options:{responsive:true,maintainAspectRatio:false,scales:{y:{beginAtZero:true,ticks:{precision:0}}}}});
}
