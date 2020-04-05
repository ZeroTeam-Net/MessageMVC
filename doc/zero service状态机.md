# start
- transport.prepare=false
> failed，start state machine
- true
> run state machine

# run end
- 如果配置为已关闭或删除
> 状态保持，实际状态机为empty
- 系统已关闭
> 状态为stop，状态机变为empty
- 否则为异常关闭
> 状态为failed，状态机为start

# zero center event
- center start
> service.start，根据状态机考虑是否可以启动，但有问题，run 结束时，状态重新可用，此时start已经调过，无法恢复。
- center stop
> service.close，然后等待realstate为close
